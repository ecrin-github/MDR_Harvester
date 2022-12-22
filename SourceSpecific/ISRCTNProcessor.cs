﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace DataHarvester.isrctn
{
    public class ISRCTNProcessor : IStudyProcessor
    {
        IMonitorDataLayer _mon_repo;
        LoggingHelper _logger;

        public ISRCTNProcessor(IMonitorDataLayer mon_repo, LoggingHelper logger)
        {
            _mon_repo = mon_repo;
            _logger = logger;
        }

        public Study ProcessData(XmlDocument d, DateTime? download_datetime)
        {
            Study s = new Study();

            List<StudyIdentifier> identifiers = new List<StudyIdentifier>();
            List<StudyTitle> titles = new List<StudyTitle>();
            List<StudyContributor> contributors = new List<StudyContributor>();
            List<StudyReference> references = new List<StudyReference>();
            List<StudyTopic> topics = new List<StudyTopic>();
            List<StudyFeature> features = new List<StudyFeature>();
            List<StudyLocation> sites = new List<StudyLocation>();
            List<StudyCountry> countries = new List<StudyCountry>();

            List<DataObject> data_objects = new List<DataObject>();
            List<ObjectTitle> object_titles = new List<ObjectTitle>();
            List<ObjectDate> object_dates = new List<ObjectDate>();
            List<ObjectInstance> object_instances = new List<ObjectInstance>();

            MD5Helpers hh = new MD5Helpers();
            StringHelpers sh = new StringHelpers(_logger);
            DateHelpers dh = new DateHelpers();
            TypeHelpers th = new TypeHelpers();
            IdentifierHelpers ih = new IdentifierHelpers();

            SplitDate reg_date = null;
            SplitDate last_edit = null;
            string study_description = null;
            string sharing_statement = null;

            // First convert the XML document to a Linq XML Document.

            XDocument xDoc = XDocument.Load(new XmlNodeReader(d));

            // Obtain the main top level elements of the registry entry.

            XElement r = xDoc.Root;

            string sid = GetElementAsString(r.Element("isctrn_id"));
            s.sd_sid = sid;
            s.datetime_of_data_fetch = download_datetime;

            // get basic study attributes
            string study_name = GetElementAsString(r.Element("study_name"));
            study_name = sh.ReplaceApos(study_name);
            s.display_title = study_name;   // = public title, default
            s.datetime_of_data_fetch = download_datetime;

            titles.Add(new StudyTitle(sid, s.display_title, 15, "Registry public title", true, "From ISRCTN"));

            // study status from trial_status and recruitment_status
            // record for now and see what is available
            string trial_status = GetElementAsString(r.Element("trial_status"));
            string recruitment_status = GetElementAsString(r.Element("recruitment_status"));
            s.study_status = trial_status + " :: recruitment :: " + recruitment_status;

            switch (trial_status)
            {
                case "Completed":
                    {
                        s.study_status = "Completed";
                        s.study_status_id = 21;
                        break;
                    }
                case "Suspended":
                    {
                        s.study_status = "Suspended";
                        s.study_status_id = 18;
                        break;
                    }
                case "Stopped":
                    {
                        s.study_status = "Terminated";
                        s.study_status_id = 22;
                        break;
                    }
                case "Ongoing":
                    {
                        switch (recruitment_status)
                        {
                            case "Not yet recruiting":
                                {
                                    s.study_status = "Not yet recruiting";
                                    s.study_status_id = 16;
                                    break;
                                }
                            case "Recruiting":
                                {
                                    s.study_status = "Recruiting";
                                    s.study_status_id = 14;
                                    break;
                                }
                            case "No longer recruiting":
                                {
                                    s.study_status = "Active, not recruiting";
                                    s.study_status_id = 15;
                                    break;
                                }
                            case "Suspended":
                                {
                                    s.study_status = "Suspended";
                                    s.study_status_id = 18;
                                    break;
                                }
                        }
                        break;
                    }
            }

            // study registry entry dates
            string r_date = GetElementAsString(r.Element("registration_date"));
            if (r_date != null)
            {
                reg_date = dh.GetDatePartsFromISOString(r_date.Substring(0, 10));
            }

            string d_edited = GetElementAsString(r.Element("last_edited"));
            if (d_edited != null)
            {
                last_edit = dh.GetDatePartsFromISOString(d_edited.Substring(0, 10));
            }


            // study sponsors
            var sponsor = r.Element("sponsor");
            if (sponsor != null)
            {
                var items = sponsor.Elements("Item");
                if (items != null && items.Count() > 0)
                {
                    foreach (XElement item in items)
                    {
                        string item_name = GetElementAsString(item.Element("item_name"));
                        string item_value = GetElementAsString(item.Element("item_value"));
                        if (item_name == "Organisation")
                        {
                            if (sh.AppearsGenuineOrgName(item_value))
                            {
                                contributors.Add(new StudyContributor(sid, 54, "Trial Sponsor",
                                    null, sh.TidyOrgName(item_value, sid)));
                            }
                        }
                    }
                }
            }

            string study_sponsor = "";
            if (contributors.Count > 0)
            {
                study_sponsor = contributors[0].organisation_name;
            }


            var contacts = r.Element("contacts");
            if (contacts != null)
            {
                var items = contacts.Elements("Item");
                if (items != null && items.Count() > 0)
                {
                    StudyContributor c = null;
                    foreach (XElement item in items)
                    {
                        string item_name = GetElementAsString(item.Element("item_name")).Trim();
                        string item_value = GetElementAsString(item.Element("item_value")).Trim();

                        switch (item_name)
                        {
                            case "Type":
                                {
                                    // starts a new contact record...
                                    // also need to store any pre-existing record
                                    if (c != null)
                                    {
                                        if (sh.CheckPersonName(c.person_full_name))
                                        {
                                            c.person_full_name = sh.TidyPersonName(c.person_full_name);
                                            if (c.person_full_name != "")
                                            {
                                                contributors.Add(c);
                                            }
                                        }
                                    }

                                    c = new StudyContributor(sid, null, null, null, null, null, null);
                                    if (item_value == "Scientific")
                                    {
                                        c.contrib_type_id = 51;
                                        c.contrib_type = "Study Lead";
                                        c.is_individual = true;
                                    }
                                    else if (item_value == "Public")
                                    {
                                        c.contrib_type_id = 56;
                                        c.contrib_type = "Public contact";
                                        c.is_individual = true;
                                    }
                                    else
                                    {
                                        c.contrib_type_id = 0;
                                        c.contrib_type = item_value;
                                        c.is_individual = true;
                                    }
                                    break;
                                }
                            case "Primary contact":
                                {
                                    c.person_full_name = sh.ReplaceApos(item_value);
                                    break;
                                }
                            case "Additional contact":
                                {
                                    c.person_full_name = sh.ReplaceApos(item_value);
                                    break;
                                }
                            case "ORCID ID":
                                {
                                    if (item_value.Contains("/"))
                                    {
                                        c.orcid_id = item_value.Substring(item_value.LastIndexOf("/") + 1);
                                    }
                                    else
                                    {
                                        c.orcid_id = item_value;
                                    }
                                    break;
                                }
                            case "email_address":
                                {
                                    // ignore...
                                    break;
                                }

                            default:
                                {
                                    // Ignore...
                                    break;
                                }
                        }
                    }

                    // do not forget the last contributor
                    if (c != null)
                    {
                        if (sh.CheckPersonName(c.person_full_name))
                        {
                            c.person_full_name = sh.TidyPersonName(c.person_full_name);
                            if (c.person_full_name != "")
                            {
                                contributors.Add(c);
                            }
                        }
                    }
                }
            }


            var funders = r.Element("funders");
            if (funders != null)
            {
                var items = funders.Elements("Item");
                if (items != null && items.Count() > 0)
                {
                    foreach (XElement item in items)
                    {
                        string item_name = GetElementAsString(item.Element("item_name")).Trim();
                        if (item_name == "Funder name")
                        {
                            string item_value = GetElementAsString(item.Element("item_value")).Trim();
                            if (sh.AppearsGenuineOrgName(item_value))
                            {
                                // check a funder is not simply the sponsor...
                                string funder = sh.TidyOrgName(item_value, sid);
                                if (funder != study_sponsor)
                                {
                                    contributors.Add(new StudyContributor(sid, 58, "Study Funder", null, funder));
                                }

                            }
                        }
                    }
                }
            }

            // study identifiers
            // do the isrctn id first...
            identifiers.Add(new StudyIdentifier(sid, sid, 11, "Trial Registry ID", 100126, "ISRCTN", reg_date?.date_string, null));

            // then any others that might be listed
            var idents = r.Element("identifiers");
            if (idents != null)
            {
                var items = idents.Elements("Item");
                if (items != null && items.Count() > 0)
                {
                    foreach (XElement item in items)
                    {
                        string item_name = GetElementAsString(item.Element("item_name")).Trim();
                        string item_value = GetElementAsString(item.Element("item_value")).Trim();

                        switch (item_name)
                        {
                            case "ClinicalTrials.gov number":
                                {
                                    identifiers.Add(new StudyIdentifier(sid, item_value, 11, "Trial Registry ID", 100120, "Clinicaltrials.gov", null, null));
                                    break;
                                }
                            case "EudraCT number":
                                {
                                    identifiers.Add(new StudyIdentifier(sid, item_value, 11, "Trial Registry ID", 100123, "EU Clinical Trials Register", null, null));
                                    break;
                                }
                            case "IRAS number":
                                {
                                    identifiers.Add(new StudyIdentifier(sid, item_value, 41, "Regulatory Body ID", 101409, "Health Research Authority", null, null));
                                    break;
                                }

                            case "Protocol/serial number":
                                {
                                    IdentifierDetails idd;
                                    if (item_value.Contains(";"))
                                    {
                                        string[] iditems = item_value.Split(";");
                                        foreach (string iditem in iditems)
                                        {
                                            string item2 = iditem.Trim();
                                            idd = ih.GetISRCTNIdentifierProps(item2, study_sponsor);
                                            if (idd.id_type != "Protocol version")
                                            {
                                                if (IsNewToList(identifiers, idd.id_value))
                                                {
                                                    identifiers.Add(new StudyIdentifier(sid, idd.id_value, idd.id_type_id, idd.id_type,
                                                                                       idd.id_org_id, idd.id_org, null, null));
                                                }
                                            }
                                        }
                                    }
                                    else if (item_value.Contains(",") &&
                                        (item_value.ToLower().Contains("iras") || item_value.ToLower().Contains("hta")))
                                    {
                                        string[] iditems = item_value.Split(",");
                                        foreach (string iditem in iditems)
                                        {
                                            string item2 = iditem.Trim();
                                            idd = ih.GetISRCTNIdentifierProps(item2, study_sponsor);
                                            if (idd.id_type != "Protocol version")
                                            {
                                                if (IsNewToList(identifiers, idd.id_value))
                                                {
                                                    identifiers.Add(new StudyIdentifier(sid, idd.id_value, idd.id_type_id, idd.id_type,
                                                                                       idd.id_org_id, idd.id_org, null, null));
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        idd = ih.GetISRCTNIdentifierProps(item_value, study_sponsor);
                                        if (idd.id_type != "Protocol version")
                                        {
                                            if (IsNewToList(identifiers, idd.id_value))
                                            {
                                                identifiers.Add(new StudyIdentifier(sid, idd.id_value, idd.id_type_id, idd.id_type,
                                                                                    idd.id_org_id, idd.id_org, null, null));
                                            }
                                        }
                                    }
                                    break;
                                }
                            default:
                                {
                                    // Ignore...
                                    break;
                                }

                        }
                    }
                }
            }

            // design info

            string listed_condition = "";   //  defined here to use in later comparison

            string PIS_details = "";
            var study_info = r.Element("study_info");
            if (study_info != null)
            {
                var items = study_info.Elements("Item");
                if (items != null && items.Count() > 0)
                {
                    foreach (XElement item in items)
                    {
                        string item_name = GetElementAsString(item.Element("item_name"));
                        string item_value = GetElementAsString(item.Element("item_value"));

                        switch (item_name)
                        {
                            case "Scientific title":
                                {
                                    string study_title = sh.ReplaceApos(item_value).Trim();
                                    if (study_title.ToLower() != study_name.ToLower())
                                    {
                                        titles.Add(new StudyTitle(sid, sh.ReplaceApos(item_value), 16, "Registry scientific title", false, "From ISRCTN"));
                                    }
                                    break;
                                }
                            case "Acronym":
                                {
                                    titles.Add(new StudyTitle(sid, item_value, 14, "Acronym or Abbreviation", false, "From ISRCTN"));
                                    break;
                                }
                            case "Study hypothesis":
                                {
                                    if (item_value != "Not provided at time of registration")
                                    {
                                        item_value = sh.StringClean(item_value);
                                        if (!item_value.ToLower().StartsWith("study"))
                                        {
                                            item_value = "Study hypothesis: " + item_value;
                                        }
                                        study_description = item_value;
                                    }
                                    break;
                                }
                            case "Primary study design":
                                {
                                    if (item_value == "Interventional")
                                    {
                                        s.study_type = "Interventional";
                                        s.study_type_id = 11;
                                    }
                                    else if (item_value == "Observational")
                                    {
                                        s.study_type = "Observational";
                                        s.study_type_id = 12;
                                    }
                                    else if (item_value == "Other")
                                    {
                                        s.study_type = "Other";
                                        s.study_type_id = 16;
                                    }
                                    break;
                                }
                            case "Secondary study design":
                                {
                                    string design = item_value.ToLower().Replace("randomized", "randomised");
                                    string design2 = design.Replace("non randomised", "non-randomised");
                                    if (design2.Contains("non-randomised"))
                                    {
                                        features.Add(new StudyFeature(sid, 22, "allocation type", 210, "Nonrandomised"));
                                    }
                                    else if (design2.Contains("randomised"))
                                    {
                                        features.Add(new StudyFeature(sid, 22, "allocation type", 205, "Randomised"));
                                    }
                                    else
                                    {
                                        features.Add(new StudyFeature(sid, 22, "allocation type", 215, "Not provided"));
                                    }
                                    break;
                                }
                            case "Trial type":
                                {
                                    int value_id = 0;
                                    string value_name = "";
                                    switch (item_value)
                                    {
                                        case "Treatment":
                                            {
                                                value_id = 400; value_name = "Treatment";
                                                break;
                                            }
                                        case "Prevention":
                                            {
                                                value_id = 405; value_name = "Prevention";
                                                break;
                                            }
                                        case "Quality of life":
                                            {
                                                value_id = 440; value_name = "Other";
                                                break;
                                            }
                                        case "Other":
                                            {
                                                value_id = 440; value_name = "Other";
                                                break;
                                            }
                                        case "Not Specified":
                                            {
                                                value_id = 445; value_name = "Not provided";
                                                break;
                                            }
                                        case "Diagnostic":
                                            {
                                                value_id = 410; value_name = "Diagnostic";
                                                break;
                                            }
                                        case "Screening":
                                            {
                                                value_id = 420; value_name = "Screening";
                                                break;
                                            }
                                    }
                                    features.Add(new StudyFeature(sid, 21, "primary purpose", value_id, value_name));
                                    break;
                                }
                            case "Study design":
                                {
                                    string design = item_value.ToLower().Replace("open label", "open-label").Replace("single blind", "single-blind");
                                    string design2 = design.Replace("double blind", "double-blind").Replace("triple blind", "triple-blind").Replace("quadruple blind", "quadruple-blind");

                                    if (design2.Contains("open-label"))
                                    {
                                        features.Add(new StudyFeature(sid, 24, "Masking", 500, "None (Open Label)"));
                                    }
                                    else if (design2.Contains("single-blind"))
                                    {
                                        features.Add(new StudyFeature(sid, 24, "Masking", 505, "Single"));
                                    }
                                    else if (design2.Contains("double-blind"))
                                    {
                                        features.Add(new StudyFeature(sid, 24, "Masking", 510, "Double"));
                                    }
                                    else if (design2.Contains("triple-blind"))
                                    {
                                        features.Add(new StudyFeature(sid, 24, "Masking", 515, "Triple"));
                                    }
                                    else if (design2.Contains("quadruple-blind"))
                                    {
                                        features.Add(new StudyFeature(sid, 24, "Masking", 520, "Quadruple"));
                                    }
                                    else
                                    {
                                        features.Add(new StudyFeature(sid, 24, "Masking", 525, "Not provided"));
                                    }

                                    string design3 = design2.Replace("case control", "case-control");

                                    if (design3.Contains("cohort"))
                                    {
                                        features.Add(new StudyFeature(sid, 30, "Observational model", 600, "Cohort"));
                                    }
                                    else if (design3.Contains("case-control"))
                                    {
                                        features.Add(new StudyFeature(sid, 30, "Observational model", 605, "Case-Control"));
                                    }
                                    else if (design3.Contains("cross section"))
                                    {
                                        features.Add(new StudyFeature(sid, 31, "Time perspective", 710, "Cross-sectional"));
                                    }
                                    else if (design3.Contains("longitudinal"))
                                    {
                                        features.Add(new StudyFeature(sid, 31, "Time perspective", 730, "Longitudinal"));
                                    }

                                    break;
                                }
                            case "Patient information sheet":
                                {
                                    if (!item_value.StartsWith("Not available") && !item_value.StartsWith("Not applicable"))
                                    {
                                        if (item_value.Contains("<a href"))
                                        {
                                            // try and create a data object later corresponding to the PIS (object and instance only)
                                            PIS_details = item_value;
                                        }
                                    }
                                    break;
                                }
                            case "Condition":
                                {
                                    listed_condition = item_value;
                                    break;
                                }
                            case "Drug names":
                                {
                                    topics.Add(new StudyTopic(sid, 12, "chemical / agent", item_value));
                                    break;
                                }
                            case "Phase":
                                {
                                    int value_id = 0;
                                    string value_name = "";
                                    switch (item_value)
                                    {
                                        case "Phase I":
                                            {
                                                value_id = 110; value_name = "Phase 1";
                                                break;
                                            }
                                        case "Phase I/II":
                                            {
                                                value_id = 115; value_name = "Phase 1/Phase 2";
                                                break;
                                            }
                                        case "Phase II":
                                            {
                                                value_id = 120; value_name = "Phase 2";
                                                break;
                                            }
                                        case "Phase II/III":
                                            {
                                                value_id = 125; value_name = "Phase 2/Phase 3";
                                                break;
                                            }
                                        case "Phase III":
                                            {
                                                value_id = 130; value_name = "Phase 3";
                                                break;
                                            }
                                        case "Phase III/IV":
                                            {
                                                value_id = 130; value_name = "Phase 3";
                                                break;
                                            }
                                        case "Phase IV":
                                            {
                                                value_id = 135; value_name = "Phase 4";
                                                break;
                                            }
                                        case "Not Specified":
                                            {
                                                value_id = 140; value_name = "Not provided";
                                                break;
                                            }
                                    }
                                    features.Add(new StudyFeature(sid, 20, "phase", value_id, value_name));
                                    break;
                                }
                            case "Primary outcome measure":
                                {
                                    if (item_value != "Not provided at time of registration")
                                    {
                                        item_value = sh.StringClean(item_value);
                                        if (!string.IsNullOrEmpty(study_description))
                                        {
                                            study_description += "\n";
                                        }
                                        if (item_value.ToLower().StartsWith("primary"))
                                        {
                                            study_description += item_value;
                                        }
                                        else
                                        {
                                            study_description += "Primary outcome(s): " + item_value;
                                        }
                                    }
                                    break;
                                }
                            case "Overall trial start date":
                                {
                                    if (item_value != "Not provided at time of registration")
                                    {
                                        CultureInfo eu_cultureinfo = new CultureInfo("fr-FR");
                                        if (DateTime.TryParse(item_value, eu_cultureinfo, DateTimeStyles.None, out DateTime start_date))
                                        {
                                            s.study_start_year = start_date.Year;
                                            s.study_start_month = start_date.Month;
                                        }
                                    }
                                    break;
                                }
                            case "Reason abandoned (if study stopped)":
                                {
                                    item_value = sh.StringClean(item_value);
                                    if (item_value != "Not provided at time of registration")
                                    {
                                        if (!string.IsNullOrEmpty(study_description))
                                        {
                                            study_description += "\n";
                                        }
                                        study_description += "Reason study stopped: " + sh.StringClean(item_value);
                                    }
                                    break;
                                }
                            case "Overall trial end date":
                                {
                                    // do nothing for now
                                    break;
                                }
                            case "Intervention type":
                                {
                                    // do nothing for now
                                    break;
                                }
                            case "Trial setting":
                                {
                                    // do nothing for now
                                    break;
                                }
                            case "Ethics approval":
                                {
                                    // do nothing for now
                                    break;
                                }
                            default:
                                {
                                    // Ignore...
                                    break;
                                }
                        }
                    }
                }
            }


            if (listed_condition != "")
            {
                topics.Add(new StudyTopic(sid, 13, "condition", listed_condition));
            }
            else
            {
                // these tend to be very general - high level classifcvations
                string conditions = GetElementAsString(r.Element("condition_category"));
                if (conditions.Contains(","))
                {
                    // add topics
                    string[] conds = conditions.Split(',');
                    for (int i = 0; i < conds.Length; i++)
                    {
                        topics.Add(new StudyTopic(sid, 13, "condition", conds[i]));
                    }
                }
                else
                {
                    // add a single topic
                    topics.Add(new StudyTopic(sid, 13, "condition", conditions));
                }
            }


            // eligibility 
            var eligibility = r.Element("eligibility");
            if (eligibility != null)
            {
                var items = eligibility.Elements("Item");
                if (items != null && items.Count() > 0)
                {
                    foreach (XElement item in items)
                    {
                        string item_name = GetElementAsString(item.Element("item_name")).Trim();
                        string item_value = GetElementAsString(item.Element("item_value")).Trim(); 

                        switch (item_name)
                        {
                            case "Age group":
                                {
                                    switch (item_value)
                                    {
                                        case "Adult":
                                            {
                                                s.min_age = 18;
                                                s.min_age_units = "Years";
                                                s.min_age_units_id = 17;
                                                s.max_age = 65;
                                                s.max_age_units = "Years";
                                                s.max_age_units_id = 17;
                                                break;
                                            }
                                        case "Senior":
                                            {
                                                s.min_age = 66;
                                                s.min_age_units = "Years";
                                                s.min_age_units_id = 17;
                                                break;
                                            }
                                        case "Neonate":
                                            {
                                                s.max_age = 28;
                                                s.max_age_units = "Days";
                                                s.max_age_units_id = 14;
                                                break;
                                            }
                                        case "Child":
                                            {
                                                s.min_age = 29;
                                                s.min_age_units = "Days";
                                                s.min_age_units_id = 14;
                                                s.max_age = 17;
                                                s.max_age_units = "Years";
                                                s.max_age_units_id = 17;
                                                break;
                                            }
                                        default:
                                            {
                                                break;
                                            }
                                    }
                                }
                                break;
                            case "Gender":
                                {
                                    switch (item_value)
                                    {
                                        case "Both":
                                            {
                                                s.study_gender_elig_id = 900;
                                                s.study_gender_elig = "All";
                                                break;
                                            }
                                        case "Female":
                                            {
                                                s.study_gender_elig_id = 905;
                                                s.study_gender_elig = "Female";
                                                break;
                                            }
                                        case "Male":
                                            {
                                                s.study_gender_elig_id = 910;
                                                s.study_gender_elig = "Male";
                                                break;
                                            }
                                        case "Not Specified":
                                            {
                                                s.study_gender_elig_id = 915;
                                                s.study_gender_elig = "Not provided";
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case "Target number of participants":
                                {
                                    if (item_value != "Not provided at time of registration")
                                    {
                                        s.study_enrolment = item_value;
                                    }
                                    break;
                                }
                            case "Total final enrolment":
                                {
                                    if (item_value != "Not provided at time of registration")
                                    {
                                        // if available also use this...
                                        if (s.study_enrolment == null)
                                        {
                                            s.study_enrolment = item_value;
                                        }
                                        else
                                        {
                                            s.study_enrolment += " (planned), " + item_value + " (final).";
                                        }
                                    }
                                    break;
                                }
                            case "Recruitment start date":
                                {
                                    // do nothing for now
                                    break;
                                }
                            case "Recruitment end date":
                                {
                                    // do nothing for now
                                    break;
                                }
                            case "Participant type":
                                {
                                    // do nothing for now
                                    break;
                                }
                            default:
                                {
                                    // Ignore...
                                    break;
                                }
                        }
                    }
                }
            }


            // locations
            var locations = r.Element("locations");
            if (locations != null)
            {
                var items = locations.Elements("Item");
                if (items != null && items.Count() > 0)
                {
                    foreach (XElement item in items)
                    {
                        string item_name = GetElementAsString(item.Element("item_name"));
                        string item_value = GetElementAsString(item.Element("item_value"));
                        switch (item_name)
                        {
                            case "Countries of recruitment": 
                                {
                                    // countries provided as a list
                                    // but some countries have a comma in them...
                                    item_value = item_value.Replace("Korea, South", "South Korea");
                                    item_value = item_value.Replace("Congo, Democratic Republic", "Democratic Republic of the Congo");

                                    string[] rec_countries = item_value.Split(",");
                                    if (rec_countries.Length > 0)
                                    {
                                        for (int i = 0; i < rec_countries.Length; i++)
                                        {
                                            string c = sh.ReplaceApos(rec_countries[i]?.Trim());
                                            if (c != "")
                                            {
                                                string c2 = c.ToLower();
                                                if (c2 == "england" || c2 == "scotland" ||
                                                    c2 == "wales" || c2 == "northern ireland")
                                                {
                                                    c = "United Kingdom";
                                                }
                                                if (c2 == "united states of america")
                                                {
                                                    c = "United States";
                                                }

                                                // Check for duplicates before adding,
                                                // especially after changes above

                                                if (countries.Count == 0)
                                                {
                                                    countries.Add(new StudyCountry(sid, c));
                                                }
                                                else
                                                {
                                                    bool add_country = true;
                                                    foreach (StudyCountry cnt in countries)
                                                    {
                                                        if (cnt.country_name == c)
                                                        {
                                                            add_country = false;
                                                            break;
                                                        }
                                                    }
                                                    if (add_country)
                                                    {
                                                        countries.Add(new StudyCountry(sid, c));
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    break;
                                }
                            case "Trial participating centre":
                                {
                                    // just the name of the centre, 
                                    // should not normally be duplicated
                                    sites.Add(new StudyLocation(sid, item_value));
                                    break;
                                }
                            default:
                                {
                                    // Ignore...
                                    break;
                                }
                        }
                    }
                }
            }


            // DATA OBJECTS and their attributes
            // initial data object is the ISRCTN registry entry

            int pub_year = 0;
            if (reg_date != null)
            {
                pub_year = (int)reg_date.year;
            }
            string object_title = "ISRCTN registry entry";
            string object_display_title = s.display_title + " :: ISRCTN registry entry";

            // create hash Id for the data object
            string sd_oid = sid + " :: 13 :: " + object_title;

            DataObject dobj = new DataObject(sd_oid, sid, object_title, object_display_title, pub_year,
                  23, "Text", 13, "Trial Registry entry", 100126, "ISRCTN", 12, download_datetime);

            dobj.doi = GetElementAsString(r.Element("doi"));
            dobj.doi_status_id = 1;
            data_objects.Add(dobj);

            // data object title is the single display title...
            object_titles.Add(new ObjectTitle(sd_oid, object_display_title,
                                                     22, "Study short name :: object type", true));
            if (last_edit != null)
            {
                object_dates.Add(new ObjectDate(sd_oid, 18, "Updated",
                                  last_edit.year, last_edit.month, last_edit.day, last_edit.date_string));
            }

            if (reg_date != null)
            {
                object_dates.Add(new ObjectDate(sd_oid, 15, "Created",
                                  reg_date.year, reg_date.month, reg_date.day, reg_date.date_string));
            }

            // instance url can be derived from the ISRCTN number
            object_instances.Add(new ObjectInstance(sd_oid, 100126, "ISRCTN",
                        "https://www.isrctn.com/" + sid, true, 35, "Web text"));



            // is there a PIS available
            if (PIS_details != "")
            {
                // PIS note includes an href to a web address
                int ref_start = PIS_details.IndexOf("href=") + 6;
                int ref_end = PIS_details.IndexOf("\"", ref_start + 1);
                string href = PIS_details.Substring(ref_start, ref_end - ref_start);

                // first check link does not provide a 404 - to be re-implemented
                if (true) //await HtmlHelpers.CheckURLAsync(href))
                {
                    int res_type_id = 35;
                    string res_type = "Web text";
                    if (href.ToLower().EndsWith("pdf"))
                    {
                        res_type_id = 11;
                        res_type = "PDF";
                    }
                    else if (href.ToLower().EndsWith("docx") || href.ToLower().EndsWith("doc"))
                    {
                        res_type_id = 16;
                        res_type = "Word doc";
                    }

                    object_title = "Patient information sheet";
                    object_display_title = s.display_title + " :: patient information sheet";
                    sd_oid = sid + " :: 19 :: " + object_title;

                    data_objects.Add(new DataObject(sd_oid, sid, object_title, object_display_title, s.study_start_year,
                      23, "Text", 19, "Patient information sheets", null, study_sponsor, 12, download_datetime));
                    object_titles.Add(new ObjectTitle(sd_oid, object_display_title,
                                                         22, "Study short name :: object type", true));
                    ObjectInstance instance = new ObjectInstance(sd_oid, null, "",
                            href, true, res_type_id, res_type);
                    instance.url_last_checked = DateTime.Today;
                    object_instances.Add(instance);
                }

            }


            // possible reference / publications
            var publications = r.Element("publications");
            if (publications != null)
            {
                var items = publications.Elements("Item");
                if (items != null && items.Count() > 0)
                {
                    foreach (XElement item in items)
                    {
                        string item_name = GetElementAsString(item.Element("item_name")).Trim();
                        string item_value = GetElementAsString(item.Element("item_value")).Trim();

                        switch (item_name)
                        {
                            case "Publication and dissemination plan":
                                {
                                    if (!item_value.Contains("Not provided at time of registration"))
                                    {
                                        if (item_value.Contains("IPD sharing statement:<br>"))
                                        {
                                            item_value = item_value.Substring(item_value.IndexOf("IPD sharing statement") + 26);
                                            sharing_statement = "IPD sharing statement: " + sh.StringClean(item_value);
                                        }

                                        else if (item_value.Contains("IPD sharing statement"))
                                        {
                                            item_value = item_value.Substring(item_value.IndexOf("IPD sharing statement") + 21);
                                            sharing_statement = "IPD sharing statement: " + sh.StringClean(item_value);
                                        }
                                        else
                                        {
                                            sharing_statement = sh.StringClean("General dissemination plan: " + item_value);
                                        }
                                    }
                                    break;
                                }
                            case "Individual participant data (IPD) sharing statement":
                                {
                                    if (!item_value.Contains("Not provided at time of registration"))
                                    {
                                        if (!string.IsNullOrEmpty(sharing_statement))
                                        {
                                            sharing_statement += "\n";
                                        }
                                        sharing_statement += sh.StringClean("IPD sharing statement: " + item_value);
                                    }
                                    break;
                                }
                            case "Participant level data":
                                {
                                    if (!item_value.Contains("Not provided at time of registration"))
                                    {
                                        if (!string.IsNullOrEmpty(sharing_statement))
                                        {
                                            sharing_statement += "\n";
                                        }
                                        sharing_statement += sh.StringClean("IPD Management: " + item_value);
                                    }
                                    break;
                                }
                           default:
                                {
                                    // Ignore...
                                    break;
                                }
                        }
                    }
                }
            }


            // possible additional files

            // this source specific utility class defined immediately below this class
            // and reset to a new collection for each study

            List<AdditionalFile> additional_files = new List<AdditionalFile>();

            var add_files = r.Element("additional_files");
            if (add_files != null)
            {
                var items = add_files.Elements("Item");
                if (items != null && items.Count() > 0)
                {
                    foreach (XElement item in items)
                    {
                        string item_name = GetElementAsString(item.Element("item_name")).Trim();
                        string item_value = GetElementAsString(item.Element("item_value")).Trim();

                        // may need to correct an extraction error here...
                        item_value = item_value.Replace("//editorial", "/editorial");

                        additional_files.Add(new AdditionalFile(sid, item_name, item_value));
                    }
                }
            }

 
            // outputs
            var outputs = r.Element("outputs");
            if (outputs != null)
            {
                var items = outputs.Elements("Output");
                if (items != null && items.Count() > 0)
                {
                    foreach (XElement item in items)
                    {
                        string output_type = GetElementAsString(item.Element("output_type")).Trim();
                        string output_url = GetElementAsString(item.Element("output_url")).Trim();
                        string details = GetElementAsString(item.Element("details")).Trim();

                        string date_created = GetElementAsString(item.Element("date_created")).Trim();
                        string date_added = GetElementAsString(item.Element("date_added")).Trim();

                        SplitDate created = null;
                        SplitDate added = null;
                        int? year_published = null;

                        if (!string.IsNullOrEmpty(date_created))
                        {
                            date_created = date_created.Substring(0, 10);
                            created = dh.GetDatePartsFromISOString(date_created);
                            year_published = created.year;
                        }
                        if (!string.IsNullOrEmpty(date_added))
                        {
                            date_added = date_added.Substring(0, 10);
                            added = dh.GetDatePartsFromISOString(date_added);
                        }

                        // correct a common past url error in download process
                        // and remove any trailing full stop, semi-colon or slash

                        if (output_url.StartsWith("https://www.isrctn.com/http"))
                        {
                            output_url = output_url.Substring(23);
                        }

                        if (output_url.EndsWith(";") || output_url.EndsWith(".")
                                || output_url.EndsWith("/"))
                        {
                            output_url = output_url.Substring(0, output_url.Length - 1);
                        }

                        // depends if they are an article, usually with a pubmed reference,
                        // or some other type of output

                        string output_lower = output_type.ToLower();
                        if (output_lower == "protocol article" || output_lower == "results article" 
                            || output_lower == "interim results article" || output_lower == "preprint results" 
                            || output_lower == "other publications" || output_lower == "abstract results" 
                            || output_lower == "abstract results" || output_lower == "thesis results " 
                            || output_lower == "thesis results" || output_lower == "protocol (preprint)"
                            || output_lower == "preprint (other)")
                        {

                            string doi = "", citation = "", pmid_string = "";
                            int pmid = 0;
                            bool pmid_found = false;

                            // try and get a pmid

                            if (output_url.Contains("pubmed"))
                            {
                                if (output_url.Contains("list_uids="))
                                {
                                    string poss_pmid = output_url.Substring(output_url.IndexOf("list_uids=") + 10);
                                    if (Int32.TryParse(poss_pmid, out pmid))
                                    {
                                        pmid_found = true;
                                    }
                                }
                                else if (output_url.Contains("termtosearch="))
                                {
                                    string poss_pmid = output_url.Substring(output_url.IndexOf("termtosearch=") + 13);
                                    if (Int32.TryParse(poss_pmid, out pmid))
                                    {
                                        pmid_found = true;
                                    }
                                }
                                else if (output_url.Contains("term="))
                                {
                                    string poss_pmid = output_url.Substring(output_url.IndexOf("term=") + 5);
                                    if (Int32.TryParse(poss_pmid, out pmid))
                                    {
                                        pmid_found = true;
                                    }
                                }
                                else
                                {
                                    // 'just' /puibmed_id at the end ...
                                    string poss_pmid = output_url.Substring(output_url.LastIndexOf("/") + 1);
                                    if (Int32.TryParse(poss_pmid, out pmid))
                                    {
                                        pmid_found = true;
                                    }
                                }

                                if (pmid_found && pmid > 0)
                                {
                                    pmid_string = pmid.ToString();
                                }
                                else
                                {
                                    citation = output_url;
                                }
                            }
                            else
                            {
                                // include the url in the citation field
                                citation = output_url;
                            }

                            // is there a doi?
                            if (output_url.Contains("doi"))
                            {
                                doi = output_url.Substring(output_url.IndexOf("doi.org/") + 8);
                            }

                            string comments = output_type.ToLower();

                            if (!string.IsNullOrEmpty(details.Trim()))
                            {
                                if ((comments == "protocol article"
                                           && details.ToLower() != "protocol" && details.ToLower() != "protocol")
                                || (comments == "results article" 
                                           && details.ToLower() != "results"))
                                {
                                    comments = comments + " (" + details + ")";
                                }
                            }

                            // add the details to the study references

                            references.Add(new StudyReference(sid, pmid_string, citation, doi, comments));
                        }
                        else
                        {
                            // need to correct a possible past extraction error here...
                            output_url = output_url.Replace("//editorial", "/editorial");

                            // create object details
                            string object_type = "";
                            int object_type_id;
                            string object_class = "Text";
                            int object_class_id = 23;

                            // One of several data object types - usually as stored by ISRCTN
                            if (output_lower == "basic results" || output_lower == "funder report results"
                                || output_lower == "thesis results" || output_lower == "poster results"
                                || output_lower == "other unpublished results" || output_lower == "book results")
                            {
                                object_type_id = 79;
                                object_type = "Results or CSR summary";
                            }
                            else if (output_lower == "protocol file" || output_lower == "protocol (other)")
                            {
                                object_type_id = 11;
                                object_type = "Study Protocol";
                            }
                            else if (output_lower == "participant information sheet")
                            {
                                object_type_id = 19;
                                object_type = "Patient information sheets";
                            }
                            else if (output_lower == "dataset")
                            {
                                object_type_id = 80;
                                object_type = "Individual participant data";
                                object_class_id = 14;
                                object_class = "Dataset";
                            }
                            else if (output_lower == "plain english results")
                            {
                                object_type_id = 88;
                                object_type = "Summary of results for public";
                            }
                            else if (output_lower.Contains("analysis"))
                            {
                                object_type_id = 22;
                                object_type = "Statistical analysis plan";
                            }
                            else if (output_lower.Contains("consent"))
                            {
                                object_type_id = 18;
                                object_type = "Informed consent forms";
                            }
                            else if (output_lower == "other files")
                            {
                                object_type_id = 37;
                                object_type = "Other text based object";
                            }
                            else if (output_lower == "trial website")
                            {
                                object_type_id = 134;
                                object_type = "Website";
                            }
                            else
                            {
                                object_type_id = 37;
                                object_type = "Other text based object";
                            }

                            // does this object exist in the additional files list - it should do...
                            // but may not be the case. If it does get the name

                            string specific_object_name = "";
                            if (additional_files.Count > 0)
                            {
                                foreach (AdditionalFile af in additional_files)
                                {
                                    if (output_url == af.item_value)
                                    {
                                        specific_object_name = af.item_name;
                                        break;
                                    }
                                }
                            }

                            int res_type_id = 0;
                            string res_type = "Not yet known";
                            int title_type_id = 0; 
                            string title_type = "Not yet known";

                            if (specific_object_name == "")
                            {
                                // may be able to derive a document title from the url
                                string url_lower = output_url.ToLower();
                                if (url_lower.Contains(".pdf"))
                                {
                                    int file_suffix_pos = url_lower.IndexOf(".pdf");
                                    int name_start_pos = output_url.LastIndexOf('/', file_suffix_pos);
                                    specific_object_name = output_url.Substring(name_start_pos + 1, file_suffix_pos + 3 - name_start_pos);
                                }
                                else if (url_lower.Contains(".docx"))
                                {
                                    int file_suffix_pos = url_lower.IndexOf(".docx");
                                    int name_start_pos = output_url.LastIndexOf("/", file_suffix_pos);
                                    specific_object_name = output_url.Substring(name_start_pos + 1, file_suffix_pos + 4 - name_start_pos);
                                }
                                else if (url_lower.Contains(".doc"))
                                {
                                    int file_suffix_pos = url_lower.IndexOf(".doc");
                                    int name_start_pos = output_url.LastIndexOf("/", file_suffix_pos);
                                    specific_object_name = output_url.Substring(name_start_pos + 1, file_suffix_pos + 3 - name_start_pos);
                                }
                                else if (url_lower.Contains(".pptx"))
                                {
                                    int file_suffix_pos = url_lower.IndexOf(".pptx");
                                    int name_start_pos = output_url.LastIndexOf("/", file_suffix_pos);
                                    specific_object_name = output_url.Substring(name_start_pos + 1, file_suffix_pos + 4 - name_start_pos);
                                }
                                else if (url_lower.Contains(".ppt"))
                                {
                                    int file_suffix_pos = url_lower.IndexOf(".ppt");
                                    int name_start_pos = output_url.LastIndexOf("/", file_suffix_pos);
                                    specific_object_name = output_url.Substring(name_start_pos + 1, file_suffix_pos + 3 - name_start_pos);
                                }
                            }


                            if (specific_object_name == "")
                            {
                                object_title = object_type;
                                object_display_title = s.display_title + " :: " + object_type;
                                sd_oid = sid + " :: " + object_type_id.ToString() + " :: " + object_type;
                                title_type_id = 22;
                                title_type = "Study short name :: object type";

                                // in almost all cases the lack of a matching document or embedded document
                                // is because the material is provided as a web page

                                if (output_url.StartsWith("http"))
                                {
                                    res_type_id = 35;
                                    res_type = "Web text";
                                }

                                // need to check if the sd_oid a duplicate?
                                // probably not as most objects should have a specific name
                            }
                            else
                            { 
                                if (specific_object_name.ToLower().EndsWith(".pdf"))
                                {
                                    res_type_id = 11;
                                    res_type = "PDF";
                                    specific_object_name = specific_object_name.Substring(0, specific_object_name.LastIndexOf("."));
                                }
                                else if (specific_object_name.ToLower().EndsWith(".docx") || specific_object_name.ToLower().EndsWith(".doc"))
                                {
                                    res_type_id = 16;
                                    res_type = "Word doc";
                                    specific_object_name = specific_object_name.Substring(0, specific_object_name.LastIndexOf("."));

                                }
                                else if (specific_object_name.ToLower().EndsWith(".pptx") || specific_object_name.ToLower().EndsWith(".ppt"))
                                {
                                    res_type_id = 20;
                                    res_type = "PowerPoint";
                                    specific_object_name = specific_object_name.Substring(0, specific_object_name.LastIndexOf("."));
                                }

                                object_title = specific_object_name;
                                object_display_title = s.display_title + " :: " + specific_object_name;
                                sd_oid = sid + " :: " + object_type_id.ToString() + " :: " + specific_object_name;
                                title_type_id = 21;
                                title_type = "Study short name :: object name";
                            }


                            // do a check that the sd_oid and resulting object name is not a duplicate
                            // if it is add a suffix before making the addition
                   
                            int next_num = 0;
                            if (data_objects.Any())
                            {
                                foreach (DataObject d_o in data_objects)
                                {
                                    if (d_o.sd_oid.StartsWith(sd_oid))
                                    {
                                        next_num++;
                                    }
                                }
                            }
                           
                            if (next_num > 0)
                            {
                                sd_oid += "_" + next_num.ToString();
                                object_display_title += "_" + next_num.ToString();
                            }

                            DataObject new_dobj = new DataObject(sd_oid, sid, object_title, object_display_title, year_published,
                                        object_class_id, object_class, object_type_id, object_type, 100126, "ISRCTN", 11, download_datetime);

                            if (details.ToLower().StartsWith("version"))
                            {
                                new_dobj.version = details;
                            }

                            data_objects.Add(new_dobj);

                            object_titles.Add(new ObjectTitle(sd_oid, object_display_title,
                                        title_type_id, title_type, true));
                            object_instances.Add(new ObjectInstance(sd_oid, 100126, "ISRCTN",
                                    output_url, true, res_type_id, res_type));

                            if (created != null)
                            {
                                object_dates.Add(new ObjectDate(sd_oid, 15, "Created", created.year, created.month, created.day, created.date_string));
                            }
                            if (added != null)
                            {
                                object_dates.Add(new ObjectDate(sd_oid, 11, "Accepted", added.year, added.month, added.day, added.date_string));
                            }
                        }
                    }
                }
            }



            // possible object of a trial web site if one exists for this study

            string trial_website = GetElementAsString(r.Element("trial_website"));
            if (!string.IsNullOrEmpty(trial_website))
            {
                // first check website link does not provide a 404
                if (true) //await HtmlHelpers.CheckURLAsync(fs.trial_website))
                {
                    object_title = "Study web site";
                    object_display_title = s.display_title + " :: Study web site";
                    sd_oid = sid + " :: 134 :: " + object_title;

                    data_objects.Add(new DataObject(sd_oid, sid, object_title, object_display_title, s.study_start_year,
                            23, "Text", 134, "Website", null, study_sponsor, 12, download_datetime));
                    object_titles.Add(new ObjectTitle(sd_oid, object_display_title,
                                                         22, "Study short name :: object type", true));
                    ObjectInstance instance = new ObjectInstance(sd_oid, null, study_sponsor,
                            trial_website, true, 35, "Web text");
                    instance.url_last_checked = DateTime.Today;
                    object_instances.Add(instance);
                }
            }


            // edit contributors - try to ensure properly categorised

            if (contributors.Count > 0)
            {
                foreach (StudyContributor sc in contributors)
                {
                    if (!sc.is_individual)
                    {
                        // identify individuals down as organisations

                        string orgname = sc.organisation_name.ToLower();
                        if (ih.CheckIfIndividual(orgname))
                        {
                            sc.person_full_name = sh.TidyPersonName(sc.organisation_name);
                            sc.organisation_name = null;
                            sc.is_individual = true;
                        }
                    }
                    else
                    {
                        // check if a group inserted as an individual

                        string fullname = sc.person_full_name.ToLower();
                        if (ih.CheckIfOrganisation(fullname))
                        {
                            sc.organisation_name = sh.TidyOrgName(sid, sc.person_full_name);
                            sc.person_full_name = null;
                            sc.is_individual = false;
                        }
                    }
                }
            }



            s.brief_description = study_description;
            s.data_sharing_statement = sharing_statement;

            s.identifiers = identifiers;
            s.titles = titles;
            s.contributors = contributors;
            s.references = references;
            s.topics = topics;
            s.features = features;
            s.sites = sites;
            s.countries = countries;

            s.data_objects = data_objects;
            s.object_titles = object_titles;
            s.object_dates = object_dates;
            s.object_instances = object_instances;

            return s;

        }


        private bool IsNewToList(List<StudyIdentifier> identifiers, string ident_value)
        {
            bool res = true;
            if (identifiers.Count > 0)
            {
                foreach (StudyIdentifier i in identifiers)
                {
                    if (ident_value == i.identifier_value)
                    {
                        res = false;
                        break;
                    }
                }
            }
            return res;
        }


        // check name...
        private int CheckObjectName(List<ObjectTitle> titles, string object_display_title)
        {
            int num_of_this_type = 0;
            if (titles.Count > 0)
            {
                for (int j = 0; j < titles.Count; j++)
                {
                    if (titles[j].title_text.Contains(object_display_title))
                    {
                        num_of_this_type++;
                    }
                }
            }
            return num_of_this_type;
        }

        private string GetElementAsString(XElement e) => (e == null) ? null : (string)e;

        private string GetAttributeAsString(XAttribute a) => (a == null) ? null : (string)a;


        private int? GetElementAsInt(XElement e)
        {
            string evalue = GetElementAsString(e);
            if (string.IsNullOrEmpty(evalue))
            {
                return null;
            }
            else
            {
                if (Int32.TryParse(evalue, out int res))
                    return res;
                else
                    return null;
            }
        }

        private int? GetAttributeAsInt(XAttribute a)
        {
            string avalue = GetAttributeAsString(a);
            if (string.IsNullOrEmpty(avalue))
            {
                return null;
            }
            else
            {
                if (Int32.TryParse(avalue, out int res))
                    return res;
                else
                    return null;
            }
        }


        private bool GetElementAsBool(XElement e)
        {
            string evalue = GetElementAsString(e);
            if (evalue != null)
            {
                return (evalue.ToLower() == "true" || evalue.ToLower()[0] == 'y') ? true : false;
            }
            else
            {
                return false;
            }
        }

        private bool GetAttributeAsBool(XAttribute a)
        {
            string avalue = GetAttributeAsString(a);
            if (avalue != null)
            {
                return (avalue.ToLower() == "true" || avalue.ToLower()[0] == 'y') ? true : false;
            }
            else
            {
                return false;
            }
        }
    }

    public class AdditionalFile
    {
        public string sd_sid { get; set; }
        public string item_name { get; set; }
        public string item_value { get; set; }

        public AdditionalFile(string _sd_sid, string _item_name, string _item_value)
        {
            sd_sid = _sd_sid;
            item_name = _item_name;
            item_value = _item_value;
        }


    }
}

