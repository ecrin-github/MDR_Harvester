﻿using PostgreSQLCopyHelper;

namespace DataHarvester
{
    public class StudyCopyHelpers
    {
        public PostgreSQLCopyHelper<StudyIdentifier> study_ids_helper =
            new PostgreSQLCopyHelper<StudyIdentifier>("sd", "study_identifiers")
                .MapVarchar("sd_sid", x => x.sd_sid)
                .MapVarchar("identifier_value", x => x.identifier_value)
                .MapInteger("identifier_type_id", x => x.identifier_type_id)
                .MapVarchar("identifier_type", x => x.identifier_type)
                .MapInteger("identifier_org_id", x => x.identifier_org_id)
                .MapVarchar("identifier_org", x => x.identifier_org)
                .MapVarchar("identifier_org_ror_id", x => x.identifier_org_ror_id)
                .MapVarchar("identifier_date", x => x.identifier_date)
                .MapVarchar("identifier_link", x => x.identifier_link);


        public PostgreSQLCopyHelper<StudyTitle> study_titles_helper =
            new PostgreSQLCopyHelper<StudyTitle>("sd", "study_titles")
                .MapVarchar("sd_sid", x => x.sd_sid)
                .MapInteger("title_type_id", x => x.title_type_id)
                .MapVarchar("title_type", x => x.title_type)
                .MapVarchar("title_text", x => x.title_text)
                .MapBoolean("is_default", x => x.is_default)
                .MapVarchar("lang_code", x => x.lang_code)
                .MapInteger("lang_usage_id", x => x.lang_usage_id)
                .MapVarchar("comments", x => x.comments);


        public PostgreSQLCopyHelper<StudyTopic> study_topics_helper =
            new PostgreSQLCopyHelper<StudyTopic>("sd", "study_topics")
                .MapVarchar("sd_sid", x => x.sd_sid)
                .MapInteger("topic_type_id", x => x.topic_type_id)
                .MapVarchar("topic_type", x => x.topic_type)
                .MapBoolean("mesh_coded", x => x.mesh_coded)
                .MapVarchar("mesh_code", x => x.mesh_code)
                .MapVarchar("mesh_value", x => x.mesh_value)
                .MapInteger("original_ct_id", x => x.original_ct_id)
                .MapVarchar("original_ct_code", x => x.original_ct_code)
                .MapVarchar("original_value", x => x.original_value);


        public PostgreSQLCopyHelper<StudyContributor> study_contributors_helper =
            new PostgreSQLCopyHelper<StudyContributor>("sd", "study_contributors")
                .MapVarchar("sd_sid", x => x.sd_sid)
                .MapInteger("contrib_type_id", x => x.contrib_type_id)
                .MapVarchar("contrib_type", x => x.contrib_type)
                .MapBoolean("is_individual", x => x.is_individual)
                .MapInteger("person_id", x => x.person_id)
                .MapVarchar("person_given_name", x => x.person_given_name)
                .MapVarchar("person_family_name", x => x.person_family_name)
                .MapVarchar("person_full_name", x => x.person_full_name)
                .MapVarchar("orcid_id", x => x.orcid_id)
                .MapVarchar("person_affiliation", x => x.person_affiliation)
                .MapInteger("organisation_id", x => x.organisation_id)
                .MapVarchar("organisation_name", x => x.organisation_name)
                .MapVarchar("organisation_ror_id", x => x.organisation_ror_id);


        public PostgreSQLCopyHelper<StudyRelationship> study_relationships_helper =
            new PostgreSQLCopyHelper<StudyRelationship>("sd", "study_relationships")
                .MapVarchar("sd_sid", x => x.sd_sid)
                .MapInteger("relationship_type_id", x => x.relationship_type_id)
                .MapVarchar("relationship_type", x => x.relationship_type)
                .MapVarchar("target_sd_sid", x => x.target_sd_sid);


        public PostgreSQLCopyHelper<StudyLink> study_links_helper =
            new PostgreSQLCopyHelper<StudyLink>("sd", "study_links")
                .MapVarchar("sd_sid", x => x.sd_sid)
                .MapVarchar("link_label", x => x.link_label)
                .MapVarchar("link_url", x => x.link_url);


        public PostgreSQLCopyHelper<StudyFeature> study_features_helper =
            new PostgreSQLCopyHelper<StudyFeature>("sd", "study_features")
                .MapVarchar("sd_sid", x => x.sd_sid)
                .MapInteger("feature_type_id", x => x.feature_type_id)
                .MapVarchar("feature_type", x => x.feature_type)
                .MapInteger("feature_value_id", x => x.feature_value_id)
                .MapVarchar("feature_value", x => x.feature_value);


        public PostgreSQLCopyHelper<StudyReference> study_references_helper =
            new PostgreSQLCopyHelper<StudyReference>("sd", "study_references")
                .MapVarchar("sd_sid", x => x.sd_sid)
                .MapVarchar("pmid", x => x.pmid)
                .MapVarchar("citation", x => x.citation)
                .MapVarchar("doi", x => x.doi)
                .MapVarchar("comments", x => x.comments);


        public PostgreSQLCopyHelper<StudyLocation> study_locations_helper =
            new PostgreSQLCopyHelper<StudyLocation>("sd", "study_locations")
                .MapVarchar("sd_sid", x => x.sd_sid)
                .MapVarchar("facility", x => x.facility)
                .MapVarchar("city_name", x => x.city_name)
                .MapVarchar("country_name", x => x.country_name)
                .MapInteger("status_id", x => x.status_id)
                .MapVarchar("status", x => x.status);


        public PostgreSQLCopyHelper<StudyCountry> study_countries_helper =
            new PostgreSQLCopyHelper<StudyCountry>("sd", "study_countries")
                .MapVarchar("sd_sid", x => x.sd_sid)
                .MapVarchar("country_name", x => x.country_name)
                .MapInteger("status_id", x => x.status_id)
                .MapVarchar("status", x => x.status);


        public PostgreSQLCopyHelper<AvailableIPD> study_avail_ipd_helper =
            new PostgreSQLCopyHelper<AvailableIPD>("sd", "study_ipd_available")
                .MapVarchar("sd_sid", x => x.sd_sid)
                .MapVarchar("ipd_id", x => x.ipd_id)
                .MapVarchar("ipd_type", x => x.ipd_type)
                .MapVarchar("ipd_url", x => x.ipd_url)
                .MapVarchar("ipd_comment", x => x.ipd_comment);

    }


    public class ObjectCopyHelpers
    {
        public PostgreSQLCopyHelper<DataObject> data_objects_helper =
            new PostgreSQLCopyHelper<DataObject>("sd", "data_objects")
                .MapVarchar("sd_oid", x => x.sd_oid)
                .MapVarchar("sd_sid", x => x.sd_sid)
                .MapVarchar("title", x => x.title)
                .MapVarchar("display_title", x => x.display_title)
                .MapVarchar("version", x => x.version)
                .MapVarchar("doi", x => x.doi)
                .MapInteger("doi_status_id", x => x.doi_status_id)
                .MapInteger("publication_year ", x => x.publication_year)
                .MapInteger("object_class_id", x => x.object_class_id)
                .MapVarchar("object_class", x => x.object_class)
                .MapInteger("object_type_id", x => x.object_type_id)
                .MapVarchar("object_type", x => x.object_type)
                .MapInteger("managing_org_id", x => x.managing_org_id)
                .MapVarchar("managing_org", x => x.managing_org)
                .MapVarchar("managing_org_ror_id", x => x.managing_org_ror_id)
                .MapVarchar("lang_code", x => x.lang_code)
                .MapInteger("access_type_id", x => x.access_type_id)
                .MapVarchar("access_type", x => x.access_type)
                .MapVarchar("access_details", x => x.access_details)
                .MapVarchar("access_details_url", x => x.access_details_url)
                .MapDate("url_last_checked", x => x.url_last_checked)
                .MapInteger("eosc_category", x => x.eosc_category)
                .MapBoolean("add_study_contribs", x => x.add_study_contribs)
                .MapBoolean("add_study_topics", x => x.add_study_topics)
                .MapTimeStampTz("datetime_of_data_fetch", x => x.datetime_of_data_fetch);


        public PostgreSQLCopyHelper<ObjectDataset> object_datasets_helper =
            new PostgreSQLCopyHelper<ObjectDataset>("sd", "object_datasets")
                .MapVarchar("sd_oid", x => x.sd_oid)
                .MapInteger("record_keys_type_id", x => x.record_keys_type_id)
                .MapVarchar("record_keys_type", x => x.record_keys_type)
                .MapVarchar("record_keys_details", x => x.record_keys_details)
                .MapInteger("deident_type_id", x => x.deident_type_id)
                .MapVarchar("deident_type", x => x.deident_type)
                .MapBoolean("deident_direct", x => x.deident_direct)
                .MapBoolean("deident_hipaa", x => x.deident_hipaa)
                .MapBoolean("deident_dates", x => x.deident_dates)
                .MapBoolean("deident_nonarr", x => x.deident_nonarr)
                .MapBoolean("deident_kanon", x => x.deident_kanon)
                .MapVarchar("deident_details", x => x.deident_details)
                .MapInteger("consent_type_id", x => x.consent_type_id)
                .MapVarchar("consent_type", x => x.consent_type)
                .MapBoolean("consent_noncommercial", x => x.consent_noncommercial)
                .MapBoolean("consent_geog_restrict", x => x.consent_geog_restrict)
                .MapBoolean("consent_research_type", x => x.consent_research_type)
                .MapBoolean("consent_genetic_only", x => x.consent_genetic_only)
                .MapBoolean("consent_no_methods", x => x.consent_no_methods)
                .MapVarchar("consent_details", x => x.consent_details);


        public PostgreSQLCopyHelper<ObjectTitle> object_titles_helper =
            new PostgreSQLCopyHelper<ObjectTitle>("sd", "object_titles")
                .MapVarchar("sd_oid", x => x.sd_oid)
                .MapInteger("title_type_id", x => x.title_type_id)
                .MapVarchar("title_type", x => x.title_type)
                .MapVarchar("title_text", x => x.title_text)
                .MapBoolean("is_default", x => x.is_default)
                .MapVarchar("lang_code", x => x.lang_code)
                .MapInteger("lang_usage_id", x => x.lang_usage_id)
                .MapVarchar("comments", x => x.comments);


        public PostgreSQLCopyHelper<ObjectInstance> object_instances_helper =
            new PostgreSQLCopyHelper<ObjectInstance>("sd", "object_instances")
                .MapVarchar("sd_oid", x => x.sd_oid)
                .MapInteger("instance_type_id", x => x.instance_type_id)
                .MapVarchar("instance_type", x => x.instance_type)
                .MapInteger("repository_org_id", x => x.repository_org_id)
                .MapVarchar("repository_org", x => x.repository_org)
                .MapVarchar("url", x => x.url)
                .MapBoolean("url_accessible", x => x.url_accessible)
                .MapDate("url_last_checked", x => x.url_last_checked)
                .MapInteger("resource_type_id", x => x.resource_type_id)
                .MapVarchar("resource_type", x => x.resource_type)
                .MapVarchar("resource_size", x => x.resource_size)
                .MapVarchar("resource_size_units", x => x.resource_size_units)
                .MapVarchar("resource_comments", x => x.resource_comments);


        public PostgreSQLCopyHelper<ObjectDate> object_dates_helper =
            new PostgreSQLCopyHelper<ObjectDate>("sd", "object_dates")
                .MapVarchar("sd_oid", x => x.sd_oid)
                .MapInteger("date_type_id", x => x.date_type_id)
                .MapVarchar("date_type", x => x.date_type)
                .MapBoolean("date_is_range", x => x.date_is_range)
                .MapInteger("start_year", x => x.start_year)
                .MapInteger("start_month", x => x.start_month)
                .MapInteger("start_day", x => x.start_day)
                .MapInteger("end_year", x => x.end_year)
                .MapInteger("end_month", x => x.end_month)
                .MapInteger("end_day", x => x.end_day)
                .MapVarchar("date_as_string", x => x.date_as_string)
                .MapVarchar("details", x => x.details);



        public PostgreSQLCopyHelper<ObjectContributor> object_contributors_helper =
            new PostgreSQLCopyHelper<ObjectContributor>("sd", "object_contributors")
                .MapVarchar("sd_oid", x => x.sd_oid)
                .MapInteger("contrib_type_id", x => x.contrib_type_id)
                .MapVarchar("contrib_type", x => x.contrib_type)
                .MapBoolean("is_individual", x => x.is_individual)
                .MapInteger("person_id", x => x.person_id)
                .MapVarchar("person_given_name", x => x.person_given_name)
                .MapVarchar("person_family_name", x => x.person_family_name)
                .MapVarchar("person_full_name", x => x.person_full_name)
                .MapVarchar("orcid_id", x => x.orcid_id)
                .MapVarchar("person_affiliation", x => x.person_affiliation)
                .MapInteger("organisation_id", x => x.organisation_id)
                .MapVarchar("organisation_name", x => x.organisation_name)
                .MapVarchar("organisation_ror_id", x => x.organisation_ror_id);


        public PostgreSQLCopyHelper<ObjectIdentifier> object_identifiers_helper =
            new PostgreSQLCopyHelper<ObjectIdentifier>("sd", "object_identifiers")
                .MapVarchar("sd_oid", x => x.sd_oid)
                .MapInteger("identifier_type_id", x => x.identifier_type_id)
                .MapVarchar("identifier_type", x => x.identifier_type)
                .MapVarchar("identifier_value", x => x.identifier_value)
                .MapInteger("identifier_org_id", x => x.identifier_org_id)
                .MapVarchar("identifier_org", x => x.identifier_org)
                .MapVarchar("identifier_org_ror_id", x => x.identifier_org_ror_id)
                .MapVarchar("identifier_date", x => x.identifier_date);


        public PostgreSQLCopyHelper<ObjectDescription> object_descriptions_helper =
            new PostgreSQLCopyHelper<ObjectDescription>("sd", "object_descriptions")
                .MapVarchar("sd_oid", x => x.sd_oid)
                .MapInteger("description_type_id", x => x.description_type_id)
                .MapVarchar("description_type", x => x.description_type)
                .MapVarchar("label", x => x.label)
                .MapVarchar("description_text", x => x.description_text)
                .MapVarchar("lang_code", x => x.lang_code);


        public PostgreSQLCopyHelper<ObjectDBLink> object_db_links_helper =
            new PostgreSQLCopyHelper<ObjectDBLink>("sd", "object_db_links")
                .MapVarchar("sd_oid", x => x.sd_oid)
                .MapInteger("db_sequence", x => x.db_sequence)
                .MapVarchar("db_name", x => x.db_name)
                .MapVarchar("id_in_db", x => x.id_in_db);


        public PostgreSQLCopyHelper<ObjectPublicationType> publication_types_helper =
            new PostgreSQLCopyHelper<ObjectPublicationType>("sd", "object_publication_types")
                .MapVarchar("sd_oid", x => x.sd_oid)
                .MapVarchar("type_name", x => x.type_name);


        public PostgreSQLCopyHelper<ObjectComment> object_comments_helper =
            new PostgreSQLCopyHelper<ObjectComment>("sd", "object_comments")
                .MapVarchar("sd_oid", x => x.sd_oid)
                .MapVarchar("ref_type", x => x.ref_type)
                .MapVarchar("ref_source", x => x.ref_source)
                .MapVarchar("pmid", x => x.pmid)
                .MapVarchar("pmid_version", x => x.pmid_version)
                .MapVarchar("notes", x => x.notes);


        public PostgreSQLCopyHelper<ObjectTopic> object_topics_helper =
            new PostgreSQLCopyHelper<ObjectTopic>("sd", "object_topics")
                .MapVarchar("sd_oid", x => x.sd_oid)
                .MapInteger("topic_type_id", x => x.topic_type_id)
                .MapVarchar("topic_type", x => x.topic_type)
                .MapBoolean("mesh_coded", x => x.mesh_coded)
                .MapVarchar("mesh_code", x => x.mesh_code)
                .MapVarchar("mesh_value", x => x.mesh_value)
                .MapInteger("original_ct_id", x => x.original_ct_id)
                .MapVarchar("original_ct_code", x => x.original_ct_code)
                .MapVarchar("original_value", x => x.original_value);


        public PostgreSQLCopyHelper<ObjectRight> object_rights_helper =
            new PostgreSQLCopyHelper<ObjectRight>("sd", "object_rights")
                .MapVarchar("sd_oid", x => x.sd_oid)
                .MapVarchar("right_name", x => x.right_name)
                .MapVarchar("right_uri", x => x.right_uri)
                .MapVarchar("notes", x => x.notes);


        public PostgreSQLCopyHelper<ObjectRelationship> object_relationships_helper =
            new PostgreSQLCopyHelper<ObjectRelationship>("sd", "object_relationships")
                .MapVarchar("sd_oid", x => x.sd_oid)
                .MapInteger("relationship_type_id", x => x.relationship_type_id)
                .MapVarchar("relationship_type", x => x.relationship_type)
                .MapVarchar("target_sd_oid", x => x.target_sd_oid);

    }
}
