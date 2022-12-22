﻿using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;

namespace DataHarvester
{
    public class Study
    {
        public string sd_sid { get; set; }
        public string display_title { get; set; }
        public string title_lang_code { get; set; }

        public string brief_description { get; set; }
        public string data_sharing_statement { get; set; }
        public int? study_start_year { get; set; }
        public int? study_start_month { get; set; }

        public int? study_type_id { get; set; }
        public string study_type { get; set; }
        public int? study_status_id { get; set; }
        public string study_status { get; set; }
        public string study_enrolment { get; set; }
        public int? study_gender_elig_id { get; set; }
        public string study_gender_elig { get; set; }

        public int? min_age { get; set; }
        public int? min_age_units_id { get; set; }
        public string min_age_units { get; set; }
        public int? max_age { get; set; }
        public int? max_age_units_id { get; set; }
        public string max_age_units { get; set; }

        public DateTime? datetime_of_data_fetch { get; set; }

        public List<StudyIdentifier> identifiers { get; set; }
        public List<StudyTitle> titles { get; set; }
        public List<StudyContributor> contributors { get; set; }
        public List<StudyReference> references { get; set; }
        public List<StudyTopic> topics { get; set; }
        public List<StudyFeature> features { get; set; }
        public List<StudyRelationship> relationships { get; set; }
        public List<StudyLocation> sites { get; set; }
        public List<StudyCountry> countries { get; set; }
        public List<StudyLink> studylinks { get; set; }
        public List<AvailableIPD> ipd_info { get; set; }

        public List<DataObject> data_objects { get; set; }
        public List<ObjectDataset> object_datasets { get; set; }
        public List<ObjectTitle> object_titles { get; set; }
        public List<ObjectDate> object_dates { get; set; }
        public List<ObjectInstance> object_instances { get; set; }

    }

    [Table("sd.studies")]
    public class StudyInDB
    {
        public string sd_sid { get; set; }
        public string display_title { get; set; }
        public string title_lang_code { get; set; }

        public string brief_description { get; set; }
        public string data_sharing_statement { get; set; }
        public int? study_start_year { get; set; }
        public int? study_start_month { get; set; }

        public int? study_type_id { get; set; }
        public string study_type { get; set; }
        public int? study_status_id { get; set; }
        public string study_status { get; set; }
        public string study_enrolment { get; set; }
        public int? study_gender_elig_id { get; set; }
        public string study_gender_elig { get; set; }

        public int? min_age { get; set; }
        public int? min_age_units_id { get; set; }
        public string min_age_units { get; set; }
        public int? max_age { get; set; }
        public int? max_age_units_id { get; set; }
        public string max_age_units { get; set; }

        public DateTime? datetime_of_data_fetch { get; set; }

        public StudyInDB(Study s)
        {
            sd_sid = s.sd_sid;
            display_title = s.display_title;
            title_lang_code = s.title_lang_code ?? "en";
            brief_description = s.brief_description;
            data_sharing_statement = s.data_sharing_statement;
            study_start_year = s.study_start_year;
            study_start_month = s.study_start_month;
            study_type_id = s.study_type_id;
            study_type = s.study_type;
            study_status_id = s.study_status_id;
            study_status = s.study_status;

            study_enrolment = s.study_enrolment;
            study_gender_elig_id = s.study_gender_elig_id;
            study_gender_elig = s.study_gender_elig;
            min_age = s.min_age;
            min_age_units_id = s.min_age_units_id;
            min_age_units = s.min_age_units;
            max_age = s.max_age;
            max_age_units_id = s.max_age_units_id;
            max_age_units = s.max_age_units;
            datetime_of_data_fetch = s.datetime_of_data_fetch;
        }
    }


    public class StudyTitle
    {
        public string sd_sid { get; set; }
        public string title_text { get; set; }
        public int? title_type_id { get; set; }
        public string title_type { get; set; }
        public string lang_code { get; set; }
        public int lang_usage_id  { get; set; }
        public bool is_default { get; set; }
        public string comments { get; set; }

        public StudyTitle(string _sd_sid, string _title_text, int? _title_type_id, string _title_type, bool _is_default)
        {
            sd_sid = _sd_sid;
            title_text = _title_text;
            title_type_id = _title_type_id;
            title_type = _title_type;
            lang_code = "en";
            lang_usage_id = 11;  // default
            is_default = _is_default;
        }

        public StudyTitle(string _sd_sid, string _title_text, int? _title_type_id, string _title_type, bool _is_default, string _comments)
        {
            sd_sid = _sd_sid;
            title_text = _title_text;
            title_type_id = _title_type_id;
            title_type = _title_type;
            lang_code = "en";
            lang_usage_id = 11;  // default
            is_default = _is_default;
            comments = _comments;
        }

        public StudyTitle(string _sd_sid, string _title_text, int? _title_type_id, string _title_type, string _lang_code, 
                               int _lang_usage_id, bool _is_default, string _comments)
        {
            sd_sid = _sd_sid;
            title_text = _title_text;
            title_type_id = _title_type_id;
            title_type = _title_type;
            lang_code = _lang_code;
            lang_usage_id = _lang_usage_id;
            is_default = _is_default;
            comments = _comments;
        }
    }


    public class StudyContributor
    {
        public string sd_sid { get; set; }
        public int? contrib_type_id { get; set; }
        public string contrib_type { get; set; }
        public bool is_individual { get; set; }
        public int? person_id { get; set; }
        public string person_given_name { get; set; }
        public string person_family_name { get; set; }
        public string person_full_name { get; set; }
        public string orcid_id { get; set; }
        public string person_affiliation { get; set; }
        public int? organisation_id { get; set; }
        public string organisation_name { get; set; }
        public string organisation_ror_id { get; set; }

        public StudyContributor(string _sd_sid, int? _contrib_type_id, string _contrib_type,
                                int? _organisation_id, string _organisation_name, string _person_full_name,
                                string _person_affiliation)
        {
            sd_sid = _sd_sid;
            contrib_type_id = _contrib_type_id;
            contrib_type = _contrib_type;
            is_individual = (_person_full_name == null) ? false : true;
            organisation_id = _organisation_id;
            organisation_name = _organisation_name;
            person_full_name = _person_full_name;
            person_affiliation = _person_affiliation;
        }

        // adding personal contributor, usually from CTG
        
        public StudyContributor(string _sd_sid, int? _contrib_type_id, string _contrib_type,
                                string _person_full_name,
                                string _person_affiliation, string _affil_organisation_name)
        {
            sd_sid = _sd_sid;
            contrib_type_id = _contrib_type_id;
            contrib_type = _contrib_type;
            is_individual = true;
            person_full_name = _person_full_name;
            person_affiliation = _person_affiliation;
            organisation_name = _affil_organisation_name;
        }

        // adding organisational contributor
        
        public StudyContributor(string _sd_sid, int? _contrib_type_id, string _contrib_type,
                                int? _organisation_id, string _organisation_name)
        {
            sd_sid = _sd_sid;
            contrib_type_id = _contrib_type_id;
            contrib_type = _contrib_type;
            is_individual = false;
            organisation_id = _organisation_id;
            organisation_name = _organisation_name;
        }

    }


    public class StudyRelationship
    {
        public string sd_sid { get; set; }
        public int relationship_type_id { get; set; }
        public string relationship_type { get; set; }
        public string target_sd_sid { get; set; }

        public StudyRelationship(string _sd_sid, int _relationship_type_id, 
                                 string _relationship_type, string _target_sd_sid)
        {
            sd_sid = _sd_sid;
            relationship_type_id = _relationship_type_id;
            relationship_type = _relationship_type;
            target_sd_sid = _target_sd_sid;
        }
    }


    public class StudyReference
    {
        public string sd_sid { get; set; }
        public string pmid { get; set; }
        public string citation { get; set; }
        public string doi { get; set; }
        public string comments { get; set; }

        public StudyReference(string _sd_sid, string _pmid, string _citation, string _doi, string _comments)
        {
            sd_sid = _sd_sid;
            pmid = _pmid;
            citation = _citation;
            doi = _doi;
            comments = _comments;
        }
    }
    

    public class StudyIdentifier
    {
        public string sd_sid { get; set; }
        public int? identifier_type_id { get; set; }
        public string identifier_type { get; set; }
        public string identifier_value { get; set; }
        public int? identifier_org_id { get; set; }
        public string identifier_org { get; set; }
        public string identifier_org_ror_id { get; set; }
        public string identifier_date { get; set; }
        public string identifier_link { get; set; }

        public StudyIdentifier() { }

        public StudyIdentifier(string _sd_sid, string _identifier_value,
            int? _identifier_type_id, string _identifier_type,
            int? _identifier_org_id, string _identifier_org)
        {
            sd_sid = _sd_sid;
            identifier_value = _identifier_value;
            identifier_type_id = _identifier_type_id;
            identifier_type = _identifier_type;
            identifier_org_id = _identifier_org_id;
            identifier_org = _identifier_org;
        }
        
        public StudyIdentifier(string _sd_sid, string _identifier_value,
            int? _identifier_type_id, string _identifier_type,
            int? _identifier_org_id, string _identifier_org,
            string _identifier_date, string _identifier_link)
        {
            sd_sid = _sd_sid;
            identifier_value = _identifier_value;
            identifier_type_id = _identifier_type_id;
            identifier_type = _identifier_type;
            identifier_org_id = _identifier_org_id;
            identifier_org = _identifier_org;
            identifier_date = _identifier_date;
            identifier_link = _identifier_link;
        }
    }


    public class StudyTopic
    {
        public string sd_sid { get; set; }
        public int topic_type_id { get; set; }
        public string topic_type { get; set; }
        public bool? mesh_coded { get; set; }
        public string mesh_code { get; set; }
        public string mesh_value { get; set; }
        public int? original_ct_id { get; set; }
        public string original_ct_code { get; set; }
        public string original_value { get; set; }

        // used for a mesh coded topic (no qualifiers for study topic codes)

        public StudyTopic(string _sd_sid, int _topic_type_id, string _topic_type,
                     bool _mesh_coded, string _mesh_code, string _mesh_value)
        {
            sd_sid = _sd_sid;
            topic_type_id = _topic_type_id;
            topic_type = _topic_type;
            mesh_coded = _mesh_coded;
            mesh_code = _mesh_code;
            mesh_value = _mesh_value;
            original_ct_id = 14;
            original_ct_code = _mesh_code;
            original_value = _mesh_value;
        }

        // non mesh coded topic - topic type and name only

        public StudyTopic(string _sd_sid, int _topic_type_id, 
                          string _topic_type, string _topic_value)
        {
            sd_sid = _sd_sid;
            topic_type_id = _topic_type_id;
            topic_type = _topic_type;
            mesh_coded = false;
            original_value = _topic_value;
        }


        // non mesh coded topic - but coded using another system - comments also possible

        public StudyTopic(string _sd_sid, int _topic_type_id, string _topic_type,
                          string _topic_value, int? _original_ct_id, 
                          string _original_ct_code)
        {
            sd_sid = _sd_sid;
            topic_type_id = _topic_type_id;
            topic_type = _topic_type;
            mesh_coded = false;
            original_ct_id = _original_ct_id;
            original_ct_code = _original_ct_code;
            original_value = _topic_value;
        }

    }


    public class StudyFeature
    {
        public string sd_sid { get; set; }
        public int? feature_type_id { get; set; }
        public string feature_type { get; set; }
        public int? feature_value_id { get; set; }
        public string feature_value { get; set; }

        public StudyFeature(string _sd_sid, int? _feature_type_id, string _feature_type, int? _feature_value_id, string _feature_value)
        {
            sd_sid = _sd_sid;
            feature_type_id = _feature_type_id;
            feature_type = _feature_type;
            feature_value_id = _feature_value_id;
            feature_value = _feature_value;
        }
    }


    public class StudyLink
    {
        public string sd_sid { get; set; }
        public string link_label { get; set; }
        public string link_url { get; set; }

        public StudyLink(string _sd_sid, string _link_label, string _link_url)
        {
            sd_sid = _sd_sid;
            link_label = _link_label;
            link_url = _link_url;
        }
    }


    public class StudyLocation
    {
        public string sd_sid { get; set; }
        public string facility { get; set; }
        public int? city_id { get; set; }
        public string city_name { get; set; }
        public int? country_id { get; set; }
        public string country_name { get; set; }
        public int? status_id { get; set; }
        public string status{ get; set; }

        public StudyLocation(string _sd_sid, string _facility, string _city_name,
                             string _country_name, int? _status_id, string _status)
        {
            sd_sid = _sd_sid;
            facility = _facility;
            city_name = _city_name;
            country_name = _country_name;
            status_id = _status_id;
            status = _status;
        }

        public StudyLocation(string _sd_sid, string _facility)
        {
            sd_sid = _sd_sid;
            facility = _facility;
        }
    }



    public class StudyCountry
    {
        public string sd_sid { get; set; }
        public int country_id { get; set; }
        public string country_name { get; set; }
        public int? status_id { get; set; }
        public string status { get; set; }

        public StudyCountry(string _sd_sid, string _country_name)
        {
            sd_sid = _sd_sid;
            country_name = _country_name;
        }

        public StudyCountry(string _sd_sid, string _country_name,
                             int? _status_id, string _status)
        {
            sd_sid = _sd_sid;
            country_name = _country_name;
            status_id = _status_id;
            status = _status;
        }
    }



    public class AvailableIPD
    {
        public string sd_sid { get; set; }
        public string ipd_id { get; set; }
        public string ipd_type { get; set; }
        public string ipd_url { get; set; }
        public string ipd_comment { get; set; }

        public AvailableIPD(string _sd_sid, string _ipd_id, string _ipd_type,
                                string _ipd_url, string _ipd_comment)
        {
            sd_sid = _sd_sid;
            ipd_id = _ipd_id;
            ipd_type = _ipd_type;
            ipd_url = _ipd_url;
            ipd_comment = _ipd_comment;
        }
    }

}
