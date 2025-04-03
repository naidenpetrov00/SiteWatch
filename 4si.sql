-- public.ac_cmd_client_addr definition

-- Drop table

-- DROP TABLE public.ac_cmd_client_addr;

CREATE TABLE public.ac_cmd_client_addr (
	"name" varchar(40) NOT NULL,
	address varchar(40) NULL,
	CONSTRAINT ac_cmd_client_addr_pkey PRIMARY KEY (name)
);
CREATE INDEX ac_cmd_client_addr_name ON public.ac_cmd_client_addr USING btree (name);


-- public.ac_cmd_dsensor definition

-- Drop table

-- DROP TABLE public.ac_cmd_dsensor;

CREATE TABLE public.ac_cmd_dsensor (
	sensor_id serial4 NOT NULL,
	client varchar(50) NULL,
	idx int2 NULL,
	"name" varchar(50) NULL,
	enabled bool DEFAULT true NOT NULL,
	inverted bool DEFAULT false NOT NULL,
	CONSTRAINT ac_cmd_dsensor_pkey PRIMARY KEY (sensor_id)
);
CREATE INDEX ac_cmd_dsensor_client_idx ON public.ac_cmd_dsensor USING btree (client);
CREATE INDEX ac_cmd_dsensor_enabled_idx ON public.ac_cmd_dsensor USING btree (enabled);
CREATE INDEX ac_cmd_dsensor_idx_idx ON public.ac_cmd_dsensor USING btree (idx);
CREATE INDEX ac_cmd_dsensor_inverted_idx ON public.ac_cmd_dsensor USING btree (inverted);
CREATE INDEX ac_cmd_dsensor_name_idx ON public.ac_cmd_dsensor USING btree (name);


-- public.ac_cmd_machine definition

-- Drop table

-- DROP TABLE public.ac_cmd_machine;

CREATE TABLE public.ac_cmd_machine (
	machine_id serial4 NOT NULL,
	machine_key varchar(120) NULL,
	client varchar(60) NULL,
	"password" varchar(90) NULL,
	added timestamptz NULL,
	approved bool DEFAULT false NOT NULL,
	CONSTRAINT ac_cmd_machine_pkey PRIMARY KEY (machine_id)
);
CREATE INDEX ac_cmd_machine_added_idx ON public.ac_cmd_machine USING btree (added);
CREATE INDEX ac_cmd_machine_approved_idx ON public.ac_cmd_machine USING btree (approved);
CREATE INDEX ac_cmd_machine_client_idx ON public.ac_cmd_machine USING btree (client);
CREATE INDEX ac_cmd_machine_machine_key_idx ON public.ac_cmd_machine USING btree (machine_key);
CREATE INDEX ac_cmd_machine_password_idx ON public.ac_cmd_machine USING btree (password);


-- public.ac_cmd_pi_printer definition

-- Drop table

-- DROP TABLE public.ac_cmd_pi_printer;

CREATE TABLE public.ac_cmd_pi_printer (
	printer_id serial4 NOT NULL,
	client varchar(50) NULL,
	"path" varchar(50) NULL,
	"name" varchar(50) NULL,
	timeout int4 DEFAULT 10 NOT NULL,
	CONSTRAINT ac_cmd_pi_printer_pkey PRIMARY KEY (printer_id)
);
CREATE INDEX ac_cmd_pi_printer_client ON public.ac_cmd_pi_printer USING btree (client);


-- public.ac_cmd_pi_reader definition

-- Drop table

-- DROP TABLE public.ac_cmd_pi_reader;

CREATE TABLE public.ac_cmd_pi_reader (
	reader_id serial4 NOT NULL,
	client varchar(90) NOT NULL,
	idx int2 DEFAULT 0 NOT NULL,
	enabled bool DEFAULT true NOT NULL,
	"name" varchar(90) NOT NULL,
	CONSTRAINT ac_cmd_pi_reader_pkey PRIMARY KEY (reader_id)
);
CREATE INDEX ac_cmd_pi_reader_client_idx ON public.ac_cmd_pi_reader USING btree (client);
CREATE INDEX ac_cmd_pi_reader_enabled_idx ON public.ac_cmd_pi_reader USING btree (enabled);
CREATE INDEX ac_cmd_pi_reader_idx_idx ON public.ac_cmd_pi_reader USING btree (idx);
CREATE INDEX ac_cmd_pi_reader_name_idx ON public.ac_cmd_pi_reader USING btree (name);


-- public.ac_cmd_pi_relay definition

-- Drop table

-- DROP TABLE public.ac_cmd_pi_relay;

CREATE TABLE public.ac_cmd_pi_relay (
	relay_id serial4 NOT NULL,
	client varchar(50) NULL,
	idx int2 NULL,
	"name" varchar(50) NULL,
	"mode" int2 NULL,
	seconds int2 NULL,
	CONSTRAINT ac_cmd_pi_relay_pkey PRIMARY KEY (relay_id)
);
CREATE INDEX ac_cmd_pi_relay_client ON public.ac_cmd_pi_relay USING btree (client);


-- public.ac_cmd_pi_serial definition

-- Drop table

-- DROP TABLE public.ac_cmd_pi_serial;

CREATE TABLE public.ac_cmd_pi_serial (
	serial_id serial4 NOT NULL,
	client varchar(50) NULL,
	"path" varchar(50) NULL,
	"name" varchar(50) NULL,
	timeout int4 DEFAULT 10 NOT NULL,
	CONSTRAINT ac_cmd_pi_serial_pkey PRIMARY KEY (serial_id)
);
CREATE INDEX ac_cmd_pi_serial_client ON public.ac_cmd_pi_serial USING btree (client);


-- public.ac_cmd_tsensor definition

-- Drop table

-- DROP TABLE public.ac_cmd_tsensor;

CREATE TABLE public.ac_cmd_tsensor (
	sensor_id serial4 NOT NULL,
	client varchar(60) NULL,
	address varchar(30) NULL,
	"name" varchar(90) NULL,
	enabled bool DEFAULT true NOT NULL,
	CONSTRAINT ac_cmd_tsensor_pkey PRIMARY KEY (sensor_id)
);
CREATE INDEX ac_cmd_tsensor_address_idx ON public.ac_cmd_tsensor USING btree (address);
CREATE INDEX ac_cmd_tsensor_client_idx ON public.ac_cmd_tsensor USING btree (client);
CREATE INDEX ac_cmd_tsensor_enabled_idx ON public.ac_cmd_tsensor USING btree (enabled);
CREATE INDEX ac_cmd_tsensor_name_idx ON public.ac_cmd_tsensor USING btree (name);


-- public.ac_device_track_machine_group definition

-- Drop table

-- DROP TABLE public.ac_device_track_machine_group;

CREATE TABLE public.ac_device_track_machine_group (
	group_id serial4 NOT NULL,
	"name" varchar(60) NULL,
	CONSTRAINT ac_device_track_machine_group_pkey PRIMARY KEY (group_id)
);
CREATE INDEX ac_device_track_machine_group_name ON public.ac_device_track_machine_group USING btree (name);


-- public.ac_device_track_machine_wgroup definition

-- Drop table

-- DROP TABLE public.ac_device_track_machine_wgroup;

CREATE TABLE public.ac_device_track_machine_wgroup (
	wgroup_id serial4 NOT NULL,
	"name" varchar(60) NULL,
	CONSTRAINT ac_device_track_machine_wgroup_pkey PRIMARY KEY (wgroup_id)
);
CREATE INDEX ac_device_track_machine_wgroup_name ON public.ac_device_track_machine_wgroup USING btree (name);


-- public.ac_device_track_pending_machine definition

-- Drop table

-- DROP TABLE public.ac_device_track_pending_machine;

CREATE TABLE public.ac_device_track_pending_machine (
	machine_id int4 NOT NULL,
	created_on timestamptz NULL,
	CONSTRAINT ac_device_track_pending_machine_pkey PRIMARY KEY (machine_id)
);
CREATE INDEX ac_device_track_pending_machine_machine_id ON public.ac_device_track_pending_machine USING btree (machine_id);


-- public.ac_device_track_support_type definition

-- Drop table

-- DROP TABLE public.ac_device_track_support_type;

CREATE TABLE public.ac_device_track_support_type (
	type_id serial4 NOT NULL,
	"name" varchar(90) NULL,
	CONSTRAINT ac_device_track_support_type_pkey PRIMARY KEY (type_id)
);
CREATE INDEX ac_device_track_support_type_name_idx ON public.ac_device_track_support_type USING btree (name);


-- public.auth_fb_user definition

-- Drop table

-- DROP TABLE public.auth_fb_user;

CREATE TABLE public.auth_fb_user (
	token_id serial4 NOT NULL,
	ref_id int4 NOT NULL,
	ref_type int2 DEFAULT 1 NOT NULL,
	"token" jsonb DEFAULT '[]'::jsonb NOT NULL,
	dates jsonb DEFAULT '[]'::jsonb NOT NULL,
	CONSTRAINT auth_fb_user_pkey PRIMARY KEY (token_id)
);
CREATE INDEX auth_fb_user_ref_id_idx ON public.auth_fb_user USING btree (ref_id);
CREATE INDEX auth_fb_user_ref_type_idx ON public.auth_fb_user USING btree (ref_type);


-- public.auth_mail_filter definition

-- Drop table

-- DROP TABLE public.auth_mail_filter;

CREATE TABLE public.auth_mail_filter (
	filter_id serial4 NOT NULL,
	users jsonb DEFAULT '[]'::jsonb NOT NULL,
	"name" varchar(70) NULL,
	regex varchar(90) NULL,
	"where" int2 DEFAULT 0 NOT NULL,
	"action" int2 DEFAULT 0 NOT NULL,
	params jsonb NULL,
	CONSTRAINT auth_mail_filter_pkey PRIMARY KEY (filter_id)
);
CREATE INDEX auth_mail_filter_action_idx ON public.auth_mail_filter USING btree (action);
CREATE INDEX auth_mail_filter_name_idx ON public.auth_mail_filter USING btree (name);
CREATE INDEX auth_mail_filter_users_idx ON public.auth_mail_filter USING btree (users);
CREATE INDEX auth_mail_filter_where_idx ON public.auth_mail_filter USING btree ("where");


-- public.auth_mail_type definition

-- Drop table

-- DROP TABLE public.auth_mail_type;

CREATE TABLE public.auth_mail_type (
	type_id serial4 NOT NULL,
	"name" varchar(90) NOT NULL,
	importance int2 DEFAULT 0 NOT NULL,
	CONSTRAINT auth_mail_type_pkey PRIMARY KEY (type_id)
);
CREATE INDEX auth_mail_type_importance_idx ON public.auth_mail_type USING btree (importance);
CREATE INDEX auth_mail_type_name_idx ON public.auth_mail_type USING btree (name);


-- public.auth_remote_machine definition

-- Drop table

-- DROP TABLE public.auth_remote_machine;

CREATE TABLE public.auth_remote_machine (
	machine_id serial4 NOT NULL,
	"key" varchar(120) NULL,
	url varchar(120) NULL,
	"name" varchar(90) NULL,
	"role" int2 DEFAULT 1 NOT NULL,
	CONSTRAINT auth_remote_machine_key_key UNIQUE (key),
	CONSTRAINT auth_remote_machine_pkey PRIMARY KEY (machine_id)
);
CREATE INDEX auth_remote_machine_key_idx ON public.auth_remote_machine USING btree (key);
CREATE INDEX auth_remote_machine_name_idx ON public.auth_remote_machine USING btree (name);
CREATE INDEX auth_remote_machine_role_idx ON public.auth_remote_machine USING btree (role);
CREATE INDEX auth_remote_machine_url_idx ON public.auth_remote_machine USING btree (url);


-- public.auth_sent_mail definition

-- Drop table

-- DROP TABLE public.auth_sent_mail;

CREATE TABLE public.auth_sent_mail (
	mail_id serial4 NOT NULL,
	user_id int4 NOT NULL,
	stamp timestamptz DEFAULT now() NOT NULL,
	"from" varchar(90) NULL,
	subject varchar(90) NULL,
	message text NULL,
	recipients text NULL,
	status bool NULL,
	stamp_read timestamptz NULL,
	rkey varchar(120) NULL,
	CONSTRAINT auth_sent_mail_pkey PRIMARY KEY (mail_id)
);
CREATE INDEX auth_sent_mail_from_idx ON public.auth_sent_mail USING btree ("from");
CREATE INDEX auth_sent_mail_recipients_idx ON public.auth_sent_mail USING btree (recipients);
CREATE INDEX auth_sent_mail_rkey_idx ON public.auth_sent_mail USING btree (rkey);
CREATE INDEX auth_sent_mail_stamp_idx ON public.auth_sent_mail USING btree (stamp);
CREATE INDEX auth_sent_mail_stamp_read_idx ON public.auth_sent_mail USING btree (stamp_read);
CREATE INDEX auth_sent_mail_status_idx ON public.auth_sent_mail USING btree (status);
CREATE INDEX auth_sent_mail_subject_idx ON public.auth_sent_mail USING btree (subject);
CREATE INDEX auth_sent_mail_user_id_idx ON public.auth_sent_mail USING btree (user_id);


-- public.base_cache definition

-- Drop table

-- DROP TABLE public.base_cache;

CREATE TABLE public.base_cache (
	"key" varchar(120) NOT NULL,
	value jsonb NOT NULL,
	expire timestamptz NULL,
	CONSTRAINT base_cache_pkey PRIMARY KEY (key)
);
CREATE INDEX base_cache_expire_idx ON public.base_cache USING btree (expire);
CREATE INDEX base_cache_key_idx ON public.base_cache USING btree (key);


-- public.base_filter definition

-- Drop table

-- DROP TABLE public.base_filter;

CREATE TABLE public.base_filter (
	bf_id serial4 NOT NULL,
	user_id int4 NOT NULL,
	lkey varchar(90) NOT NULL,
	"name" varchar(120) NOT NULL,
	settings jsonb DEFAULT '{}'::jsonb NOT NULL,
	CONSTRAINT base_filter_pkey PRIMARY KEY (bf_id),
	CONSTRAINT base_filter_user_id_lkey_name_key UNIQUE (user_id, lkey, name)
);
CREATE INDEX base_filter_lkey_idx ON public.base_filter USING btree (lkey);
CREATE INDEX base_filter_name_idx ON public.base_filter USING btree (name);
CREATE INDEX base_filter_user_id_idx ON public.base_filter USING btree (user_id);


-- public.base_listing definition

-- Drop table

-- DROP TABLE public.base_listing;

CREATE TABLE public.base_listing (
	user_id int4 NOT NULL,
	lkey varchar(90) NOT NULL,
	settings jsonb NULL,
	cur_filter varchar(120) NULL,
	CONSTRAINT base_listing_pkey PRIMARY KEY (user_id, lkey)
);
CREATE INDEX base_listing_lkey_idx ON public.base_listing USING btree (lkey);
CREATE INDEX base_listing_user_id_idx ON public.base_listing USING btree (user_id);


-- public.base_meta definition

-- Drop table

-- DROP TABLE public.base_meta;

CREATE TABLE public.base_meta (
	host varchar(100) NULL,
	build varchar(100) NULL
);


-- public.base_notification definition

-- Drop table

-- DROP TABLE public.base_notification;

CREATE TABLE public.base_notification (
	notification_id serial4 NOT NULL,
	"key" text NOT NULL,
	value text NULL,
	"date" timestamptz DEFAULT now() NOT NULL,
	CONSTRAINT base_notification_pkey PRIMARY KEY (notification_id)
);
CREATE INDEX base_notification_date_idx ON public.base_notification USING btree (date);
CREATE INDEX base_notification_key_idx ON public.base_notification USING btree (key);


-- public.base_notification_icon definition

-- Drop table

-- DROP TABLE public.base_notification_icon;

CREATE TABLE public.base_notification_icon (
	icon_id serial4 NOT NULL,
	"source" varchar(60) NULL,
	CONSTRAINT base_notification_icon_pkey PRIMARY KEY (icon_id)
);
CREATE INDEX base_notification_icon_source ON public.base_notification_icon USING btree (source);


-- public.base_package definition

-- Drop table

-- DROP TABLE public.base_package;

CREATE TABLE public.base_package (
	"name" varchar(100) NOT NULL,
	"version" int4 NOT NULL,
	CONSTRAINT base_package_name_key UNIQUE (name)
);
CREATE INDEX base_package_name_idx ON public.base_package USING btree (name);


-- public.base_win_sizes definition

-- Drop table

-- DROP TABLE public.base_win_sizes;

CREATE TABLE public.base_win_sizes (
	user_id int4 NOT NULL,
	lkey varchar(90) NOT NULL,
	settings jsonb NULL,
	CONSTRAINT base_win_sizes_pkey PRIMARY KEY (user_id, lkey)
);
CREATE INDEX base_win_sizes_lkey_idx ON public.base_win_sizes USING btree (lkey);
CREATE INDEX base_win_sizes_user_id_idx ON public.base_win_sizes USING btree (user_id);


-- public.carrier definition

-- Drop table

-- DROP TABLE public.carrier;

CREATE TABLE public.carrier (
	carrier_id smallserial NOT NULL,
	"name" varchar(60) NOT NULL,
	active int2 DEFAULT 0 NOT NULL,
	settings json NULL,
	CONSTRAINT carrier_pkey PRIMARY KEY (carrier_id)
);


-- public.case_archive definition

-- Drop table

-- DROP TABLE public.case_archive;

CREATE TABLE public.case_archive (
	arch_id serial4 NOT NULL,
	"name" varchar(120) NULL,
	CONSTRAINT case_archive_pkey PRIMARY KEY (arch_id)
);
CREATE INDEX case_archive_name_idx ON public.case_archive USING btree (name);


-- public.case_bnb_interest definition

-- Drop table

-- DROP TABLE public.case_bnb_interest;

CREATE TABLE public.case_bnb_interest (
	stamp timestamptz NOT NULL,
	coef numeric(12, 3) DEFAULT 0 NOT NULL,
	CONSTRAINT case_bnb_interest_pkey PRIMARY KEY (stamp)
);
CREATE INDEX case_bnb_interest_stamp_idx ON public.case_bnb_interest USING btree (stamp);


-- public.case_case_import definition

-- Drop table

-- DROP TABLE public.case_case_import;

CREATE TABLE public.case_case_import (
	case_number varchar(18) NOT NULL,
	flags int4 DEFAULT 0 NOT NULL,
	CONSTRAINT case_case_import_pkey PRIMARY KEY (case_number)
);
CREATE INDEX case_case_import_case_number_idx ON public.case_case_import USING btree (case_number);


-- public.case_comments definition

-- Drop table

-- DROP TABLE public.case_comments;

CREATE TABLE public.case_comments (
	comment_id serial4 NOT NULL,
	ref_id int4 NOT NULL,
	type_id int2 DEFAULT 0 NOT NULL,
	creator_id int4 NOT NULL,
	"comment" text NULL,
	created timestamptz DEFAULT now() NOT NULL,
	address_id jsonb DEFAULT '[]'::jsonb NOT NULL,
	CONSTRAINT case_comments_pkey PRIMARY KEY (comment_id)
);
CREATE INDEX case_comments_address_id_idx ON public.case_comments USING btree (address_id);
CREATE INDEX case_comments_created_idx ON public.case_comments USING btree (created);
CREATE INDEX case_comments_creator_id_idx ON public.case_comments USING btree (creator_id);
CREATE INDEX case_comments_ref_id_idx ON public.case_comments USING btree (ref_id);
CREATE INDEX case_comments_type_id_idx ON public.case_comments USING btree (type_id);


-- public.case_deliverer definition

-- Drop table

-- DROP TABLE public.case_deliverer;

CREATE TABLE public.case_deliverer (
	del_id serial4 NOT NULL,
	"type" int2 DEFAULT 0 NOT NULL,
	"name" varchar(120) NULL,
	username varchar(100) NULL,
	"password" varchar(60) NULL,
	price numeric(12, 3) DEFAULT 0 NOT NULL,
	currency_id int2 DEFAULT 1 NOT NULL,
	CONSTRAINT case_deliverer_pkey PRIMARY KEY (del_id)
);
CREATE INDEX case_deliverer_type_idx ON public.case_deliverer USING btree (type);
CREATE INDEX case_deliverer_username_idx ON public.case_deliverer USING btree (username);


-- public.case_doc_type definition

-- Drop table

-- DROP TABLE public.case_doc_type;

CREATE TABLE public.case_doc_type (
	type_id serial4 NOT NULL,
	"name" varchar(120) NULL,
	doc_nr int4 DEFAULT 0 NOT NULL,
	CONSTRAINT case_doc_type_pkey PRIMARY KEY (type_id)
);
CREATE INDEX case_doc_type_doc_nr_idx ON public.case_doc_type USING btree (doc_nr);
CREATE INDEX case_doc_type_name_idx ON public.case_doc_type USING btree (name);


-- public.case_doc_type_pack definition

-- Drop table

-- DROP TABLE public.case_doc_type_pack;

CREATE TABLE public.case_doc_type_pack (
	pack_id serial4 NOT NULL,
	"name" varchar(140) NOT NULL,
	type_ids jsonb DEFAULT '[]'::jsonb NOT NULL,
	stype_ids jsonb DEFAULT '[]'::jsonb NOT NULL,
	CONSTRAINT case_doc_type_pack_pkey PRIMARY KEY (pack_id)
);
CREATE INDEX case_doc_type_pack_name_idx ON public.case_doc_type_pack USING btree (name);


-- public.case_input_checksum definition

-- Drop table

-- DROP TABLE public.case_input_checksum;

CREATE TABLE public.case_input_checksum (
	iid bigserial NOT NULL,
	"type" int2 DEFAULT 1 NOT NULL,
	checksum varchar(120) NULL,
	CONSTRAINT case_input_checksum_pkey PRIMARY KEY (iid)
);
CREATE INDEX case_input_checksum_checksum_idx ON public.case_input_checksum USING btree (checksum);
CREATE INDEX case_input_checksum_type_idx ON public.case_input_checksum USING btree (type);


-- public.case_municipality definition

-- Drop table

-- DROP TABLE public.case_municipality;

CREATE TABLE public.case_municipality (
	mun_id serial4 NOT NULL,
	country_id int4 NOT NULL,
	zone_id int4 NOT NULL,
	"name" varchar(120) NOT NULL,
	CONSTRAINT case_municipality_pkey PRIMARY KEY (mun_id)
);
CREATE INDEX case_municipality_country_id_idx ON public.case_municipality USING btree (country_id);
CREATE INDEX case_municipality_name_idx ON public.case_municipality USING btree (name);
CREATE INDEX case_municipality_zone_id_idx ON public.case_municipality USING btree (zone_id);


-- public.case_person definition

-- Drop table

-- DROP TABLE public.case_person;

CREATE TABLE public.case_person (
	person_id serial4 NOT NULL,
	group_ids jsonb DEFAULT '[]'::jsonb NOT NULL,
	active bool DEFAULT true NOT NULL,
	first_name varchar(120) NULL,
	middle_name varchar(120) NULL,
	last_name varchar(120) NULL,
	phone varchar(90) NULL,
	mail varchar(70) NULL,
	picture varchar(70) NULL,
	pid varchar(70) NULL,
	pid_type int2 DEFAULT 0 NOT NULL,
	passport_nr varchar(70) NULL,
	passport_end_date timestamptz NULL,
	id_card_nr varchar(70) NULL,
	id_card_end_date timestamptz NULL,
	created timestamptz DEFAULT now() NOT NULL,
	modified timestamptz DEFAULT now() NOT NULL,
	created_id int4 NOT NULL,
	modified_id int4 NOT NULL,
	gender int2 DEFAULT 0 NOT NULL,
	birthday timestamptz NULL,
	company varchar(120) NULL,
	uid varchar(90) NULL,
	uid_vat varchar(90) NULL,
	tsv tsvector NULL,
	case_type int2 DEFAULT 5 NOT NULL,
	check_data jsonb DEFAULT '{}'::jsonb NOT NULL,
	b_type int2 DEFAULT '-1'::integer::smallint NOT NULL,
	passed timestamptz NULL,
	married bool NULL,
	user_ids jsonb DEFAULT '[]'::jsonb NOT NULL,
	CONSTRAINT case_person_pkey PRIMARY KEY (person_id)
);
CREATE INDEX case_person_active_idx ON public.case_person USING btree (active);
CREATE INDEX case_person_birthday_idx ON public.case_person USING btree (birthday);
CREATE INDEX case_person_company_idx ON public.case_person USING btree (company);
CREATE INDEX case_person_created_id_idx ON public.case_person USING btree (created_id);
CREATE INDEX case_person_created_idx ON public.case_person USING btree (created);
CREATE INDEX case_person_first_name_idx ON public.case_person USING btree (first_name);
CREATE INDEX case_person_gender_idx ON public.case_person USING btree (gender);
CREATE INDEX case_person_group_ids_idx ON public.case_person USING btree (group_ids);
CREATE INDEX case_person_id_card_end_date_idx ON public.case_person USING btree (id_card_end_date);
CREATE INDEX case_person_id_card_nr_idx ON public.case_person USING btree (id_card_nr);
CREATE INDEX case_person_last_name_idx ON public.case_person USING btree (last_name);
CREATE INDEX case_person_mail_idx ON public.case_person USING btree (mail);
CREATE INDEX case_person_married_idx ON public.case_person USING btree (married);
CREATE INDEX case_person_middle_name_idx ON public.case_person USING btree (middle_name);
CREATE INDEX case_person_modified_id_idx ON public.case_person USING btree (modified_id);
CREATE INDEX case_person_modified_idx ON public.case_person USING btree (modified);
CREATE INDEX case_person_passed_idx ON public.case_person USING btree (passed);
CREATE INDEX case_person_passport_end_date_idx ON public.case_person USING btree (passport_end_date);
CREATE INDEX case_person_passport_nr_idx ON public.case_person USING btree (passport_nr);
CREATE INDEX case_person_phone_idx ON public.case_person USING btree (phone);
CREATE INDEX case_person_picture_idx ON public.case_person USING btree (picture);
CREATE INDEX case_person_pid_idx ON public.case_person USING btree (pid);
CREATE INDEX case_person_pid_type_idx ON public.case_person USING btree (pid_type);
CREATE INDEX case_person_tsv_idx ON public.case_person USING btree (tsv);
CREATE INDEX case_person_uid_idx ON public.case_person USING btree (uid);
CREATE INDEX case_person_uid_vat_idx ON public.case_person USING btree (uid_vat);
CREATE INDEX case_person_user_ids_idx ON public.case_person USING btree (user_ids);

-- Table Triggers

create trigger person_tsv_vector before
insert
    or
update
    on
    public.case_person for each row execute function person_tsv_vector();


-- public.case_person_group definition

-- Drop table

-- DROP TABLE public.case_person_group;

CREATE TABLE public.case_person_group (
	group_id serial4 NOT NULL,
	"name" varchar(120) NULL,
	CONSTRAINT case_person_group_pkey PRIMARY KEY (group_id)
);
CREATE INDEX case_person_group_name_idx ON public.case_person_group USING btree (name);


-- public.case_task_type definition

-- Drop table

-- DROP TABLE public.case_task_type;

CREATE TABLE public.case_task_type (
	type_id serial4 NOT NULL,
	"name" varchar(120) NULL,
	CONSTRAINT case_task_type_pkey PRIMARY KEY (type_id)
);
CREATE INDEX case_task_type_name_idx ON public.case_task_type USING btree (name);


-- public.case_tech_matter_type definition

-- Drop table

-- DROP TABLE public.case_tech_matter_type;

CREATE TABLE public.case_tech_matter_type (
	type_id serial4 NOT NULL,
	kind int2 DEFAULT 1 NOT NULL,
	descr varchar(120) NULL,
	CONSTRAINT case_tech_matter_type_pkey PRIMARY KEY (type_id)
);
CREATE INDEX case_tech_matter_type_descr_idx ON public.case_tech_matter_type USING btree (descr);
CREATE INDEX case_tech_matter_type_kind_idx ON public.case_tech_matter_type USING btree (kind);


-- public.case_vhcl_brand definition

-- Drop table

-- DROP TABLE public.case_vhcl_brand;

CREATE TABLE public.case_vhcl_brand (
	brand_id serial4 NOT NULL,
	"name" varchar(120) NOT NULL,
	CONSTRAINT case_vhcl_brand_pkey PRIMARY KEY (brand_id)
);
CREATE INDEX case_vhcl_brand_name_idx ON public.case_vhcl_brand USING btree (name);


-- public.chat_client definition

-- Drop table

-- DROP TABLE public.chat_client;

CREATE TABLE public.chat_client (
	chat_client_id serial4 NOT NULL,
	"name" varchar(60) DEFAULT ''::character varying NOT NULL,
	mail varchar(60) DEFAULT ''::character varying NOT NULL,
	"date" timestamp DEFAULT now() NOT NULL,
	CONSTRAINT chat_client_pkey PRIMARY KEY (chat_client_id)
);


-- public.chat_client_message definition

-- Drop table

-- DROP TABLE public.chat_client_message;

CREATE TABLE public.chat_client_message (
	chat_client_message_id serial4 NOT NULL,
	chat_client_session_id int4 NOT NULL,
	side int2 NOT NULL,
	message text DEFAULT ''::text NOT NULL,
	"date" timestamp DEFAULT now() NOT NULL,
	CONSTRAINT chat_client_message_pkey PRIMARY KEY (chat_client_message_id)
);


-- public.chat_client_session definition

-- Drop table

-- DROP TABLE public.chat_client_session;

CREATE TABLE public.chat_client_session (
	chat_client_session_id serial4 NOT NULL,
	chat_client_id int4 NOT NULL,
	user_id int4 NOT NULL,
	closed int2 NOT NULL,
	date_start timestamp DEFAULT now() NOT NULL,
	date_end timestamp DEFAULT now() NOT NULL,
	CONSTRAINT chat_client_session_pkey PRIMARY KEY (chat_client_session_id)
);
CREATE INDEX chat_client_session_chat_client_id ON public.chat_client_session USING btree (chat_client_id);


-- public.chat_room definition

-- Drop table

-- DROP TABLE public.chat_room;

CREATE TABLE public.chat_room (
	chat_room_id serial4 NOT NULL,
	"name" text NULL,
	context text NULL,
	CONSTRAINT chat_room_pkey PRIMARY KEY (chat_room_id)
);
CREATE INDEX chat_room_context_idx ON public.chat_room USING btree (context);


-- public.city_details definition

-- Drop table

-- DROP TABLE public.city_details;

CREATE TABLE public.city_details (
	city_id serial4 NOT NULL,
	city varchar(80) NULL,
	"admin" varchar(80) NULL,
	country_id int4 NOT NULL,
	population_proper int4 NULL,
	population int4 NULL,
	iso2 varchar(10) NULL,
	capital int2 NULL,
	lat numeric(12, 8) NULL,
	lng numeric(12, 8) NULL,
	mui_city varchar(200) NULL,
	timezone varchar(70) NULL,
	CONSTRAINT city_details_pkey PRIMARY KEY (city_id)
);
CREATE INDEX city_details_city ON public.city_details USING btree (city);
CREATE INDEX city_details_country_id ON public.city_details USING btree (country_id);
CREATE INDEX city_details_mui_city ON public.city_details USING btree (mui_city);
CREATE INDEX city_details_timezone ON public.city_details USING btree (timezone);


-- public.cl_expense_type definition

-- Drop table

-- DROP TABLE public.cl_expense_type;

CREATE TABLE public.cl_expense_type (
	type_id serial4 NOT NULL,
	"name" varchar(60) NULL,
	CONSTRAINT cl_expense_type_pkey PRIMARY KEY (type_id)
);
CREATE INDEX cl_expense_type_name ON public.cl_expense_type USING btree (name);


-- public.cl_nkpd_code definition

-- Drop table

-- DROP TABLE public.cl_nkpd_code;

CREATE TABLE public.cl_nkpd_code (
	code int4 NOT NULL,
	descr varchar(80) NULL,
	CONSTRAINT cl_nkpd_code_pkey PRIMARY KEY (code)
);
CREATE INDEX cl_nkpd_code_code ON public.cl_nkpd_code USING btree (code);
CREATE INDEX cl_nkpd_code_descr ON public.cl_nkpd_code USING btree (descr);


-- public.cl_reg_event_type definition

-- Drop table

-- DROP TABLE public.cl_reg_event_type;

CREATE TABLE public.cl_reg_event_type (
	event_type_id serial4 NOT NULL,
	"name" varchar(60) NOT NULL,
	CONSTRAINT cl_reg_event_type_pkey PRIMARY KEY (event_type_id)
);


-- public.cl_salary_car definition

-- Drop table

-- DROP TABLE public.cl_salary_car;

CREATE TABLE public.cl_salary_car (
	car_id serial4 NOT NULL,
	reg varchar(20) NULL,
	descr varchar(90) NULL,
	fuel int2 DEFAULT 1 NOT NULL,
	km_start numeric(12, 2) DEFAULT 0 NOT NULL,
	fuel_start numeric(12, 2) DEFAULT 0 NOT NULL,
	stamp timestamptz DEFAULT now() NOT NULL,
	library_id int4 NULL,
	warehouse_id int4 NOT NULL,
	office_id int4 NOT NULL,
	built timestamptz NULL,
	min_lth numeric(12, 2) DEFAULT 0 NOT NULL,
	max_lth numeric(12, 2) DEFAULT 0 NOT NULL,
	CONSTRAINT cl_salary_car_pkey PRIMARY KEY (car_id)
);
CREATE INDEX cl_salary_car_built_idx ON public.cl_salary_car USING btree (built);
CREATE INDEX cl_salary_car_descr_idx ON public.cl_salary_car USING btree (descr);
CREATE INDEX cl_salary_car_fuel_idx ON public.cl_salary_car USING btree (fuel);
CREATE INDEX cl_salary_car_library_id_idx ON public.cl_salary_car USING btree (library_id);
CREATE INDEX cl_salary_car_office_id_idx ON public.cl_salary_car USING btree (office_id);
CREATE INDEX cl_salary_car_reg_idx ON public.cl_salary_car USING btree (reg);
CREATE INDEX cl_salary_car_stamp_idx ON public.cl_salary_car USING btree (stamp);
CREATE INDEX cl_salary_car_warehouse_id_idx ON public.cl_salary_car USING btree (warehouse_id);


-- public.cl_salary_document definition

-- Drop table

-- DROP TABLE public.cl_salary_document;

CREATE TABLE public.cl_salary_document (
	doc_id serial4 NOT NULL,
	creator_id int4 NOT NULL,
	created timestamptz DEFAULT now() NOT NULL,
	title varchar(110) NULL,
	body text NULL,
	CONSTRAINT cl_salary_document_pkey PRIMARY KEY (doc_id)
);
CREATE INDEX cl_salary_document_created_idx ON public.cl_salary_document USING btree (created);
CREATE INDEX cl_salary_document_creator_id_idx ON public.cl_salary_document USING btree (creator_id);
CREATE INDEX cl_salary_document_title_idx ON public.cl_salary_document USING btree (title);


-- public.cl_salary_holiday_req definition

-- Drop table

-- DROP TABLE public.cl_salary_holiday_req;

CREATE TABLE public.cl_salary_holiday_req (
	req_id serial4 NOT NULL,
	created timestamptz DEFAULT now() NOT NULL,
	creator_id int4 NOT NULL,
	starts timestamptz NOT NULL,
	ends timestamptz NOT NULL,
	worker_id int4 NOT NULL,
	"locked" bool DEFAULT false NOT NULL,
	locked_id int4 NULL,
	task_id jsonb DEFAULT '[]'::jsonb NOT NULL,
	lock_notify_ids jsonb DEFAULT '[]'::jsonb NOT NULL,
	params jsonb NULL,
	CONSTRAINT cl_salary_holiday_req_pkey PRIMARY KEY (req_id)
);
CREATE INDEX cl_salary_holiday_req_created_idx ON public.cl_salary_holiday_req USING btree (created);
CREATE INDEX cl_salary_holiday_req_creator_id_idx ON public.cl_salary_holiday_req USING btree (creator_id);
CREATE INDEX cl_salary_holiday_req_ends_idx ON public.cl_salary_holiday_req USING btree (ends);
CREATE INDEX cl_salary_holiday_req_locked_id_idx ON public.cl_salary_holiday_req USING btree (locked_id);
CREATE INDEX cl_salary_holiday_req_locked_idx ON public.cl_salary_holiday_req USING btree (locked);
CREATE INDEX cl_salary_holiday_req_starts_idx ON public.cl_salary_holiday_req USING btree (starts);
CREATE INDEX cl_salary_holiday_req_worker_id_idx ON public.cl_salary_holiday_req USING btree (worker_id);


-- public.clt_task definition

-- Drop table

-- DROP TABLE public.clt_task;

CREATE TABLE public.clt_task (
	task_id serial4 NOT NULL,
	enabled bool DEFAULT true NOT NULL,
	"name" text NOT NULL,
	company_id int4 NULL,
	customer_id int4 NULL,
	created_id int4 NULL,
	modified_id int4 NULL,
	locked_id int4 NULL,
	office_id int4 NULL,
	"locked" bool DEFAULT false NOT NULL,
	"action" int4 DEFAULT 0 NOT NULL,
	dstart timestamptz NOT NULL,
	dend timestamptz NOT NULL,
	type_id int4 NOT NULL,
	status int4 NULL,
	event_id jsonb DEFAULT '[]'::jsonb NOT NULL,
	user_id jsonb DEFAULT '[]'::jsonb NOT NULL,
	watcher_id jsonb DEFAULT '[]'::jsonb NOT NULL,
	group_id jsonb DEFAULT '[]'::jsonb NOT NULL,
	responsible_id jsonb DEFAULT '[]'::jsonb NOT NULL,
	"key" varchar(90) NULL,
	"comment" text NULL,
	lock_notify_ids jsonb DEFAULT '[]'::jsonb NOT NULL,
	comment_notify_ids jsonb DEFAULT '[]'::jsonb NOT NULL,
	attach_notify_ids jsonb DEFAULT '[]'::jsonb NOT NULL,
	recurring jsonb NULL,
	created_stamp timestamptz DEFAULT now() NOT NULL,
	locked_stamp timestamptz NULL,
	daily_notify jsonb DEFAULT '[]'::jsonb NOT NULL,
	library_id int4 NULL,
	settings jsonb DEFAULT '{}'::jsonb NOT NULL,
	CONSTRAINT clt_task_pkey PRIMARY KEY (task_id)
);
CREATE INDEX clt_task_company_id ON public.clt_task USING btree (company_id);
CREATE INDEX clt_task_created_id ON public.clt_task USING btree (created_id);
CREATE INDEX clt_task_customer_id ON public.clt_task USING btree (customer_id);
CREATE INDEX clt_task_locked_id ON public.clt_task USING btree (locked_id);
CREATE INDEX clt_task_modified_id ON public.clt_task USING btree (modified_id);
CREATE INDEX clt_task_office_id ON public.clt_task USING btree (office_id);
CREATE INDEX clt_task_status ON public.clt_task USING btree (status);
CREATE INDEX clt_task_type_id ON public.clt_task USING btree (type_id);


-- public.clt_task_type definition

-- Drop table

-- DROP TABLE public.clt_task_type;

CREATE TABLE public.clt_task_type (
	task_type_id serial4 NOT NULL,
	description varchar(60) NOT NULL,
	settings jsonb NULL,
	CONSTRAINT clt_task_type_pkey PRIMARY KEY (task_type_id)
);
CREATE INDEX clt_task_type_description_idx ON public.clt_task_type USING btree (description);


-- public.contr_cnotification definition

-- Drop table

-- DROP TABLE public.contr_cnotification;

CREATE TABLE public.contr_cnotification (
	not_id bigserial NOT NULL,
	title varchar(90) NULL,
	body text NULL,
	created timestamptz DEFAULT now() NOT NULL,
	CONSTRAINT contr_cnotification_pkey PRIMARY KEY (not_id)
);
CREATE INDEX contr_cnotification_created_idx ON public.contr_cnotification USING btree (created);
CREATE INDEX contr_cnotification_title_idx ON public.contr_cnotification USING btree (title);


-- public.contr_division definition

-- Drop table

-- DROP TABLE public.contr_division;

CREATE TABLE public.contr_division (
	div_id serial4 NOT NULL,
	"name" varchar(120) NOT NULL,
	CONSTRAINT contr_division_pkey PRIMARY KEY (div_id)
);
CREATE INDEX contr_division_name_idx ON public.contr_division USING btree (name);


-- public.contr_position definition

-- Drop table

-- DROP TABLE public.contr_position;

CREATE TABLE public.contr_position (
	position_id serial4 NOT NULL,
	flags int4 NULL,
	"name" varchar(50) NULL,
	CONSTRAINT contr_position_pkey PRIMARY KEY (position_id)
);


-- public.contr_signature definition

-- Drop table

-- DROP TABLE public.contr_signature;

CREATE TABLE public.contr_signature (
	sig_id serial4 NOT NULL,
	ref_id int4 NOT NULL,
	ref_type int2 DEFAULT 1 NOT NULL,
	points jsonb NULL,
	CONSTRAINT contr_signature_pkey PRIMARY KEY (sig_id)
);
CREATE INDEX contr_signature_ref_id_idx ON public.contr_signature USING btree (ref_id);
CREATE INDEX contr_signature_ref_type_idx ON public.contr_signature USING btree (ref_type);


-- public.country definition

-- Drop table

-- DROP TABLE public.country;

CREATE TABLE public.country (
	country_id serial4 NOT NULL,
	"name" varchar(128) DEFAULT ''::character varying NOT NULL,
	iso bpchar(2) DEFAULT ''::bpchar NOT NULL,
	geo_lat numeric(12, 4) NULL,
	geo_lng numeric(12, 4) NULL,
	active int2 DEFAULT 0 NOT NULL,
	tele_code varchar(10) NULL,
	CONSTRAINT country_pkey PRIMARY KEY (country_id)
);
CREATE INDEX country_active_idx ON public.country USING btree (active);
CREATE INDEX country_iso_idx ON public.country USING btree (iso);
CREATE INDEX country_name_idx ON public.country USING btree (name);
CREATE INDEX country_tele_code_idx ON public.country USING btree (tele_code);


-- public.country_zone definition

-- Drop table

-- DROP TABLE public.country_zone;

CREATE TABLE public.country_zone (
	country_zone_id serial4 NOT NULL,
	country_id int4 NOT NULL,
	"name" varchar(128) DEFAULT ''::character varying NOT NULL,
	iso bpchar(5) DEFAULT ''::bpchar NOT NULL,
	geo_lat numeric(12, 4) NULL,
	geo_lng numeric(12, 4) NULL,
	active int2 DEFAULT 0 NOT NULL,
	CONSTRAINT country_zone_pkey PRIMARY KEY (country_zone_id)
);
CREATE INDEX country_zone_active_idx ON public.country_zone USING btree (active);
CREATE INDEX country_zone_country_id ON public.country_zone USING btree (country_id);
CREATE INDEX country_zone_iso_idx ON public.country_zone USING btree (iso);
CREATE INDEX country_zone_name_idx ON public.country_zone USING btree (name);


-- public.credit_discount definition

-- Drop table

-- DROP TABLE public.credit_discount;

CREATE TABLE public.credit_discount (
	discount_id serial4 NOT NULL,
	from_amount numeric(12, 2) NULL,
	to_amount numeric(12, 2) NULL,
	price numeric(12, 2) NULL,
	price_currency_id int4 NOT NULL,
	CONSTRAINT credit_discount_pkey PRIMARY KEY (discount_id)
);
CREATE INDEX credit_discount_from_amount_idx ON public.credit_discount USING btree (from_amount);
CREATE INDEX credit_discount_price_currency_id_idx ON public.credit_discount USING btree (price_currency_id);
CREATE INDEX credit_discount_price_idx ON public.credit_discount USING btree (price);
CREATE INDEX credit_discount_to_amount_idx ON public.credit_discount USING btree (to_amount);


-- public.credit_purse_type definition

-- Drop table

-- DROP TABLE public.credit_purse_type;

CREATE TABLE public.credit_purse_type (
	type_id serial4 NOT NULL,
	"limit" numeric(12, 2) NULL,
	"name" varchar(90) NULL,
	CONSTRAINT credit_purse_type_pkey PRIMARY KEY (type_id)
);
CREATE INDEX credit_purse_type_name ON public.credit_purse_type USING btree (type_id);


-- public.currency definition

-- Drop table

-- DROP TABLE public.currency;

CREATE TABLE public.currency (
	currency_id smallserial NOT NULL,
	title varchar(20) DEFAULT ''::character varying NOT NULL,
	symbol varchar(10) DEFAULT ''::character varying NOT NULL,
	symbol_position int2 DEFAULT 0 NOT NULL,
	round int2 DEFAULT 2 NOT NULL,
	active bool DEFAULT false NOT NULL,
	is_default bool DEFAULT false NOT NULL,
	thousands int2 DEFAULT 0 NOT NULL,
	CONSTRAINT currency_pkey PRIMARY KEY (currency_id)
);
CREATE INDEX currency_active_idx ON public.currency USING btree (active);
CREATE INDEX currency_is_default_idx ON public.currency USING btree (is_default);
CREATE INDEX currency_symbol_idx ON public.currency USING btree (symbol);
CREATE INDEX currency_thousands_idx ON public.currency USING btree (thousands);
CREATE INDEX currency_title_idx ON public.currency USING btree (title);


-- public.currency_data definition

-- Drop table

-- DROP TABLE public.currency_data;

CREATE TABLE public.currency_data (
	currency_id int2 NOT NULL,
	currency_history_id int4 NOT NULL,
	rate numeric(12, 5) DEFAULT 0 NOT NULL,
	CONSTRAINT currency_data_pkey PRIMARY KEY (currency_id, currency_history_id)
);


-- public.currency_history definition

-- Drop table

-- DROP TABLE public.currency_history;

CREATE TABLE public.currency_history (
	currency_history_id serial4 NOT NULL,
	"date" timestamptz DEFAULT now() NOT NULL,
	CONSTRAINT currency_history_pkey PRIMARY KEY (currency_history_id)
);


-- public.cust_relation definition

-- Drop table

-- DROP TABLE public.cust_relation;

CREATE TABLE public.cust_relation (
	rel_id serial4 NOT NULL,
	blood bool DEFAULT false NOT NULL,
	descr varchar(90) NULL,
	CONSTRAINT cust_relation_pkey PRIMARY KEY (rel_id)
);
CREATE INDEX cust_relation_blood_idx ON public.cust_relation USING btree (blood);
CREATE INDEX cust_relation_descr_idx ON public.cust_relation USING btree (descr);


-- public.customer_group definition

-- Drop table

-- DROP TABLE public.customer_group;

CREATE TABLE public.customer_group (
	customer_group_id serial4 NOT NULL,
	"name" varchar(60) NOT NULL,
	tax_client_id int2 DEFAULT 0 NOT NULL,
	is_default bool DEFAULT false NOT NULL,
	CONSTRAINT customer_group_pkey PRIMARY KEY (customer_group_id)
);
CREATE INDEX customer_group_is_default_idx ON public.customer_group USING btree (is_default);
CREATE INDEX customer_group_name_idx ON public.customer_group USING btree (name);
CREATE INDEX customer_group_tax_client_id_idx ON public.customer_group USING btree (tax_client_id);


-- public."dictionary" definition

-- Drop table

-- DROP TABLE public."dictionary";

CREATE TABLE public."dictionary" (
	dictionary_id serial4 NOT NULL,
	"name" varchar(100) NOT NULL,
	intl jsonb NOT NULL,
	CONSTRAINT dictionary_name_key UNIQUE (name),
	CONSTRAINT dictionary_pkey PRIMARY KEY (dictionary_id)
);


-- public.elist_invoice_map definition

-- Drop table

-- DROP TABLE public.elist_invoice_map;

CREATE TABLE public.elist_invoice_map (
	case_number varchar(80) NULL,
	invoice_ids jsonb NULL
);


-- public.event_context definition

-- Drop table

-- DROP TABLE public.event_context;

CREATE TABLE public.event_context (
	context_id serial4 NOT NULL,
	title text NULL,
	CONSTRAINT event_context_pkey PRIMARY KEY (context_id)
);
CREATE INDEX event_context_title ON public.event_context USING btree (title);
CREATE INDEX event_context_title_idx ON public.event_context USING btree (title);


-- public.famap definition

-- Drop table

-- DROP TABLE public.famap;

CREATE TABLE public.famap (
	"oid" varchar(40) NULL,
	nid jsonb DEFAULT '[]'::jsonb NOT NULL
);


-- public.invmap definition

-- Drop table

-- DROP TABLE public.invmap;

CREATE TABLE public.invmap (
	"oid" varchar(40) NULL,
	nid int4 NULL
);


-- public."language" definition

-- Drop table

-- DROP TABLE public."language";

CREATE TABLE public."language" (
	language_id smallserial NOT NULL,
	"name" varchar(128) DEFAULT ''::character varying NOT NULL,
	name_local varchar(128) NULL,
	code bpchar(2) DEFAULT ''::bpchar NOT NULL,
	locale bpchar(5) DEFAULT ''::bpchar NOT NULL,
	active bool DEFAULT false NOT NULL,
	"position" int2 DEFAULT 0 NOT NULL,
	script int2 DEFAULT 0 NOT NULL,
	CONSTRAINT language_code_key UNIQUE (code),
	CONSTRAINT language_locale_key UNIQUE (locale),
	CONSTRAINT language_pkey PRIMARY KEY (language_id)
);
CREATE INDEX language_active_idx ON public.language USING btree (active);
CREATE INDEX language_code_idx ON public.language USING btree (code);
CREATE INDEX language_locale_idx ON public.language USING btree (locale);
CREATE INDEX language_name_idx ON public.language USING btree (name);
CREATE INDEX language_name_local_idx ON public.language USING btree (name_local);
CREATE INDEX language_position_idx ON public.language USING btree ("position");
CREATE INDEX language_script_idx ON public.language USING btree (script);


-- public.library_file_entry definition

-- Drop table

-- DROP TABLE public.library_file_entry;

CREATE TABLE public.library_file_entry (
	entry_id serial4 NOT NULL,
	"path" varchar(100) NULL,
	parent_id int4 NULL,
	allow_flags jsonb NULL,
	deny_flags jsonb NULL,
	CONSTRAINT library_file_entry_path_parent_id_key UNIQUE (path, parent_id),
	CONSTRAINT library_file_entry_pkey PRIMARY KEY (entry_id)
);
CREATE INDEX library_file_entry_parent_id ON public.library_file_entry USING btree (parent_id);
CREATE INDEX library_file_entry_path ON public.library_file_entry USING btree (path);


-- public.library_imported_fs definition

-- Drop table

-- DROP TABLE public.library_imported_fs;

CREATE TABLE public.library_imported_fs (
	import_id serial4 NOT NULL,
	entry_id int4 NOT NULL,
	"key" varchar(90) NULL,
	"name" varchar(90) NULL,
	ip varchar(30) NULL,
	CONSTRAINT library_imported_fs_pkey PRIMARY KEY (import_id)
);
CREATE INDEX library_imported_fs_entry_id_idx ON public.library_imported_fs USING btree (entry_id);
CREATE INDEX library_imported_fs_ip_idx ON public.library_imported_fs USING btree (ip);
CREATE INDEX library_imported_fs_key_idx ON public.library_imported_fs USING btree (key);
CREATE INDEX library_imported_fs_name_idx ON public.library_imported_fs USING btree (name);


-- public.local_settings definition

-- Drop table

-- DROP TABLE public.local_settings;

CREATE TABLE public.local_settings (
	"key" varchar(90) NOT NULL,
	value jsonb NULL,
	CONSTRAINT local_settings_pkey PRIMARY KEY (key)
);
CREATE INDEX local_settings_key_idx ON public.local_settings USING btree (key);


-- public.office definition

-- Drop table

-- DROP TABLE public.office;

CREATE TABLE public.office (
	office_id serial4 NOT NULL,
	title varchar(100) DEFAULT ''::character varying NOT NULL,
	logo varchar(50) DEFAULT ''::character varying NOT NULL,
	address varchar(100) DEFAULT ''::character varying NOT NULL,
	CONSTRAINT office_pkey PRIMARY KEY (office_id)
);


-- public.oprod_map definition

-- Drop table

-- DROP TABLE public.oprod_map;

CREATE TABLE public.oprod_map (
	oprod_id varchar(60) NULL,
	nprod_id int4 NOT NULL
);


-- public.ordmap definition

-- Drop table

-- DROP TABLE public.ordmap;

CREATE TABLE public.ordmap (
	"oid" varchar(40) NULL,
	nid int4 NULL
);


-- public.paymap definition

-- Drop table

-- DROP TABLE public.paymap;

CREATE TABLE public.paymap (
	"oid" varchar(40) NULL,
	nid jsonb DEFAULT '[]'::jsonb NOT NULL
);


-- public.payment definition

-- Drop table

-- DROP TABLE public.payment;

CREATE TABLE public.payment (
	payment_id serial4 NOT NULL,
	"name" varchar(30) DEFAULT ''::character varying NOT NULL,
	intl jsonb NOT NULL,
	settings jsonb NULL,
	active int2 DEFAULT 0 NOT NULL,
	is_default bool DEFAULT false NOT NULL,
	CONSTRAINT payment_pkey PRIMARY KEY (payment_id)
);
CREATE INDEX payment_active_idx ON public.payment USING btree (active);
CREATE INDEX payment_is_default_idx ON public.payment USING btree (is_default);
CREATE INDEX payment_name_idx ON public.payment USING btree (name);


-- public.personnel_group_schedule definition

-- Drop table

-- DROP TABLE public.personnel_group_schedule;

CREATE TABLE public.personnel_group_schedule (
	schedule_id serial4 NOT NULL,
	schedule_data jsonb NULL,
	"name" varchar(50) NULL,
	CONSTRAINT personnel_group_schedule_pkey PRIMARY KEY (schedule_id)
);
CREATE INDEX personnel_group_schedule_name ON public.personnel_group_schedule USING btree (name);


-- public.personnel_worker_report definition

-- Drop table

-- DROP TABLE public.personnel_worker_report;

CREATE TABLE public.personnel_worker_report (
	report_id serial4 NOT NULL,
	worker_id int4 NOT NULL,
	tstart timestamptz NULL,
	tend timestamptz NULL,
	leave_minutes jsonb NULL,
	work_minutes int4 NULL,
	break_minutes int4 NULL,
	modifier_id int4 NULL,
	modify_date timestamptz NULL,
	daily_work_minutes jsonb NULL,
	CONSTRAINT personnel_worker_report_pkey PRIMARY KEY (report_id)
);
CREATE INDEX personnel_worker_report_tend ON public.personnel_worker_report USING btree (tend);
CREATE INDEX personnel_worker_report_tstart ON public.personnel_worker_report USING btree (tstart);
CREATE INDEX personnel_worker_report_worker ON public.personnel_worker_report USING btree (worker_id);


-- public.pidmaps definition

-- Drop table

-- DROP TABLE public.pidmaps;

CREATE TABLE public.pidmaps (
	oldid varchar(90) NULL,
	newid int8 NULL
);
CREATE INDEX pidmaps_newid_idx ON public.pidmaps USING btree (newid);
CREATE INDEX pidmaps_oldid_idx ON public.pidmaps USING btree (oldid);


-- public.prod_day_serial definition

-- Drop table

-- DROP TABLE public.prod_day_serial;

CREATE TABLE public.prod_day_serial (
	serial_id serial4 NOT NULL,
	seq int4 NOT NULL,
	"year" int4 DEFAULT date_part('year'::text, now()) NOT NULL,
	"month" int4 DEFAULT date_part('month'::text, now()) NOT NULL,
	"day" int4 DEFAULT date_part('day'::text, now()) NOT NULL,
	CONSTRAINT prod_day_serial_pkey PRIMARY KEY (serial_id),
	CONSTRAINT prod_day_serial_seq_year_month_day_key UNIQUE (seq, year, month, day)
);
CREATE INDEX prod_day_serial_day_idx ON public.prod_day_serial USING btree (day);
CREATE INDEX prod_day_serial_month_idx ON public.prod_day_serial USING btree (month);
CREATE INDEX prod_day_serial_seq_idx ON public.prod_day_serial USING btree (seq);
CREATE INDEX prod_day_serial_year_idx ON public.prod_day_serial USING btree (year);


-- public.product_attribute definition

-- Drop table

-- DROP TABLE public.product_attribute;

CREATE TABLE public.product_attribute (
	product_attribute_id serial4 NOT NULL,
	"type" int2 NOT NULL,
	"ref" varchar(100) NULL,
	title varchar(100) NOT NULL,
	"filter" bool DEFAULT false NOT NULL,
	visible bool DEFAULT false NOT NULL,
	product_attribute_sub_id int4 NULL,
	"position" int4 NULL,
	CONSTRAINT product_attribute_pkey PRIMARY KEY (product_attribute_id),
	CONSTRAINT product_attribute_ref_key UNIQUE (ref),
	CONSTRAINT product_attribute_title_key UNIQUE (title)
);
CREATE INDEX product_attribute_position ON public.product_attribute USING btree ("position");
CREATE INDEX product_attribute_ref ON public.product_attribute USING btree (ref);
CREATE INDEX product_attribute_sub_sub_id ON public.product_attribute USING btree (product_attribute_sub_id);
CREATE INDEX product_attribute_title ON public.product_attribute USING btree (title);


-- public.product_deleted definition

-- Drop table

-- DROP TABLE public.product_deleted;

CREATE TABLE public.product_deleted (
	product_id int4 NOT NULL,
	"data" text DEFAULT ''::text NOT NULL,
	CONSTRAINT product_deleted_pkey PRIMARY KEY (product_id)
);


-- public.product_group definition

-- Drop table

-- DROP TABLE public.product_group;

CREATE TABLE public.product_group (
	product_group_id serial4 NOT NULL,
	parent_product_group_id int4 DEFAULT 1 NOT NULL,
	title varchar(100) NOT NULL,
	lft int4 DEFAULT 0 NOT NULL,
	rgt int4 DEFAULT 0 NOT NULL,
	"position" int4 DEFAULT 0 NOT NULL,
	code varchar(100) NULL,
	"comment" text NULL,
	active bool DEFAULT false NOT NULL,
	products int4 DEFAULT 0 NOT NULL,
	date_created timestamptz DEFAULT now() NOT NULL,
	date_modified timestamptz DEFAULT now() NOT NULL,
	tsv tsvector NULL,
	CONSTRAINT product_group_check CHECK ((parent_product_group_id <> product_group_id)),
	CONSTRAINT product_group_pkey PRIMARY KEY (product_group_id)
);
CREATE INDEX product_group_lft ON public.product_group USING btree (lft);
CREATE INDEX product_group_rgt ON public.product_group USING btree (rgt);
CREATE INDEX product_group_title ON public.product_group USING btree (title);
CREATE INDEX product_group_tsv ON public.product_group USING gin (tsv);

-- Table Triggers

create trigger product_group_delete before
delete
    on
    public.product_group for each row
    when ((pg_trigger_depth() = 0)) execute function product_group_delete();
create trigger product_group_insert after
insert
    on
    public.product_group for each row
    when ((new.parent_product_group_id <> 0)) execute function product_group_insert();
create trigger product_group_tsv_vector before
insert
    or
update
    on
    public.product_group for each row execute function product_group_tsv_vector();
create trigger product_group_update after
update
    on
    public.product_group for each row
    when ((old.parent_product_group_id is distinct
from
    new.parent_product_group_id)) execute function product_group_update();


-- public.product_manufacturer definition

-- Drop table

-- DROP TABLE public.product_manufacturer;

CREATE TABLE public.product_manufacturer (
	man_id serial4 NOT NULL,
	"name" varchar(120) NOT NULL,
	CONSTRAINT product_manufacturer_pkey PRIMARY KEY (man_id)
);
CREATE INDEX product_manufacturer_name_idx ON public.product_manufacturer USING btree (name);


-- public.promo_coupon definition

-- Drop table

-- DROP TABLE public.promo_coupon;

CREATE TABLE public.promo_coupon (
	promo_coupon_id serial4 NOT NULL,
	code varchar(10) NOT NULL,
	title varchar(100) DEFAULT ''::character varying NOT NULL,
	discount numeric(12, 4) DEFAULT 0 NOT NULL,
	discount_type int2 DEFAULT 0 NOT NULL,
	date_start timestamptz NULL,
	date_end timestamptz NULL,
	uses_total int4 DEFAULT 0 NOT NULL,
	uses_customer int4 DEFAULT 0 NOT NULL,
	amount numeric(12, 4) DEFAULT 0 NOT NULL,
	active bool DEFAULT false NOT NULL,
	CONSTRAINT promo_coupon_pkey PRIMARY KEY (promo_coupon_id)
);
CREATE INDEX code_promo_coupon ON public.promo_coupon USING btree (code);


-- public.promo_discount definition

-- Drop table

-- DROP TABLE public.promo_discount;

CREATE TABLE public.promo_discount (
	promo_discount_id serial4 NOT NULL,
	title varchar(100) DEFAULT ''::character varying NOT NULL,
	discount numeric(12, 4) DEFAULT 0 NOT NULL,
	discount_type int2 DEFAULT 0 NOT NULL,
	date_start timestamptz NULL,
	date_end timestamptz NULL,
	priority int4 DEFAULT 0 NOT NULL,
	"rule" jsonb NOT NULL,
	active bool DEFAULT false NOT NULL,
	CONSTRAINT promo_discount_pkey PRIMARY KEY (promo_discount_id)
);
CREATE INDEX title_promo_discount ON public.promo_discount USING btree (title);


-- public.purchase_status definition

-- Drop table

-- DROP TABLE public.purchase_status;

CREATE TABLE public.purchase_status (
	status_id serial4 NOT NULL,
	state int4 DEFAULT 0 NOT NULL,
	"name" varchar(60) NULL,
	CONSTRAINT purchase_status_pkey PRIMARY KEY (status_id)
);
CREATE INDEX purchase_status_state ON public.purchase_status USING btree (state);


-- public.purchase_type definition

-- Drop table

-- DROP TABLE public.purchase_type;

CREATE TABLE public.purchase_type (
	type_id serial4 NOT NULL,
	"name" varchar(70) NULL,
	require_sale bool DEFAULT false NOT NULL,
	CONSTRAINT purchase_type_pkey PRIMARY KEY (type_id)
);
CREATE INDEX purchase_type_name_idx ON public.purchase_type USING btree (name);
CREATE INDEX purchase_type_require_sale_idx ON public.purchase_type USING btree (require_sale);


-- public.purse definition

-- Drop table

-- DROP TABLE public.purse;

CREATE TABLE public.purse (
	purse_id serial4 NOT NULL,
	user_id int4 NOT NULL,
	bank_account_id int4 NULL,
	company_id int4 NULL,
	"name" varchar(60) NULL,
	sums jsonb NULL,
	CONSTRAINT purse_pkey PRIMARY KEY (purse_id)
);
CREATE INDEX purse_company_id ON public.purse USING btree (company_id);
CREATE INDEX purse_user_id ON public.purse USING btree (user_id);


-- public.purse_arrived_money definition

-- Drop table

-- DROP TABLE public.purse_arrived_money;

CREATE TABLE public.purse_arrived_money (
	arr_id bigserial NOT NULL,
	amount numeric(12, 3) DEFAULT 0 NOT NULL,
	currency_id int2 DEFAULT 1 NOT NULL,
	operation_id varchar(90) NULL,
	stamp timestamptz DEFAULT now() NOT NULL,
	sender_id int4 NULL,
	sender_name varchar(120) NULL,
	customer_id int4 NULL,
	reason text NULL,
	bank_id int4 DEFAULT '-1'::integer NOT NULL,
	iban varchar(60) NULL,
	sender_iban varchar(60) NULL,
	sender_bank_code varchar(20) NULL,
	created_id int4 NOT NULL,
	modified_id int4 NOT NULL,
	created timestamptz DEFAULT now() NOT NULL,
	modified timestamptz DEFAULT now() NOT NULL,
	ref_ids jsonb DEFAULT '[]'::jsonb NOT NULL,
	ref_types jsonb DEFAULT '[]'::jsonb NOT NULL,
	ref_id int4 NULL,
	ref_type int2 NULL,
	applied bool DEFAULT false NOT NULL,
	company_id int4 NOT NULL,
	parent_id int8 NULL,
	CONSTRAINT purse_arrived_money_pkey PRIMARY KEY (arr_id)
);
CREATE INDEX purse_arrived_money_amount_idx ON public.purse_arrived_money USING btree (amount);
CREATE INDEX purse_arrived_money_applied_idx ON public.purse_arrived_money USING btree (applied);
CREATE INDEX purse_arrived_money_bank_id_idx ON public.purse_arrived_money USING btree (bank_id);
CREATE INDEX purse_arrived_money_created_id_idx ON public.purse_arrived_money USING btree (created_id);
CREATE INDEX purse_arrived_money_created_idx ON public.purse_arrived_money USING btree (created);
CREATE INDEX purse_arrived_money_currency_id_idx ON public.purse_arrived_money USING btree (currency_id);
CREATE INDEX purse_arrived_money_customer_id_idx ON public.purse_arrived_money USING btree (customer_id);
CREATE INDEX purse_arrived_money_iban_idx ON public.purse_arrived_money USING btree (iban);
CREATE INDEX purse_arrived_money_modified_id_idx ON public.purse_arrived_money USING btree (modified_id);
CREATE INDEX purse_arrived_money_modified_idx ON public.purse_arrived_money USING btree (modified);
CREATE INDEX purse_arrived_money_parent_id_idx ON public.purse_arrived_money USING btree (parent_id);
CREATE INDEX purse_arrived_money_ref_id_idx ON public.purse_arrived_money USING btree (ref_id);
CREATE INDEX purse_arrived_money_ref_ids_idx ON public.purse_arrived_money USING btree (ref_ids);
CREATE INDEX purse_arrived_money_ref_type_idx ON public.purse_arrived_money USING btree (ref_type);
CREATE INDEX purse_arrived_money_ref_types_idx ON public.purse_arrived_money USING btree (ref_types);
CREATE INDEX purse_arrived_money_sender_bank_code_idx ON public.purse_arrived_money USING btree (sender_bank_code);
CREATE INDEX purse_arrived_money_sender_iban_idx ON public.purse_arrived_money USING btree (sender_iban);
CREATE INDEX purse_arrived_money_sender_id_idx ON public.purse_arrived_money USING btree (sender_id);
CREATE INDEX purse_arrived_money_sender_name_idx ON public.purse_arrived_money USING btree (sender_name);
CREATE INDEX purse_arrived_money_stamp_idx ON public.purse_arrived_money USING btree (stamp);


-- public.purse_transaction definition

-- Drop table

-- DROP TABLE public.purse_transaction;

CREATE TABLE public.purse_transaction (
	transaction_id serial4 NOT NULL,
	from_purse_id int4 NULL,
	to_purse_id int4 NULL,
	modified_id int4 NOT NULL,
	modified_time timestamptz DEFAULT now() NOT NULL,
	"type" int4 DEFAULT 0 NOT NULL,
	payment_id int4 NULL,
	sums jsonb NULL,
	"comment" text NULL,
	person_id int4 NULL,
	person_type int2 DEFAULT '0'::smallint NOT NULL,
	CONSTRAINT purse_transaction_pkey PRIMARY KEY (transaction_id)
);
CREATE INDEX purse_transaction_from_purse_id ON public.purse_transaction USING btree (from_purse_id);
CREATE INDEX purse_transaction_modified_id ON public.purse_transaction USING btree (modified_id);
CREATE INDEX purse_transaction_payment_id ON public.purse_transaction USING btree (payment_id);
CREATE INDEX purse_transaction_person_id_idx ON public.purse_transaction USING btree (person_id);
CREATE INDEX purse_transaction_person_type_idx ON public.purse_transaction USING btree (person_type);
CREATE INDEX purse_transaction_to_purse_id ON public.purse_transaction USING btree (to_purse_id);
CREATE INDEX purse_transaction_type ON public.purse_transaction USING btree (type);


-- public.purse_wait_list definition

-- Drop table

-- DROP TABLE public.purse_wait_list;

CREATE TABLE public.purse_wait_list (
	wait_id serial4 NOT NULL,
	price numeric(12, 2) NULL,
	price_currency_id int4 NOT NULL,
	customer_id int4 NOT NULL,
	tcreate timestamptz NULL,
	tapprove timestamptz NULL,
	sale_id int4 NULL,
	transaction_id int4 NULL,
	CONSTRAINT purse_wait_list_pkey PRIMARY KEY (wait_id)
);
CREATE INDEX purse_wait_list_customer_id_idx ON public.purse_wait_list USING btree (customer_id);
CREATE INDEX purse_wait_list_price_idx ON public.purse_wait_list USING btree (price);
CREATE INDEX purse_wait_list_sale_id_idx ON public.purse_wait_list USING btree (sale_id);
CREATE INDEX purse_wait_list_tapprove_idx ON public.purse_wait_list USING btree (tapprove);
CREATE INDEX purse_wait_list_tcreate_idx ON public.purse_wait_list USING btree (tcreate);
CREATE INDEX purse_wait_list_transaction_id_idx ON public.purse_wait_list USING btree (transaction_id);


-- public.quantity_unit definition

-- Drop table

-- DROP TABLE public.quantity_unit;

CREATE TABLE public.quantity_unit (
	quantity_unit_id serial4 NOT NULL,
	unit varchar(30) DEFAULT ''::character varying NOT NULL,
	intl jsonb NOT NULL,
	value numeric(12, 5) DEFAULT 0 NOT NULL,
	CONSTRAINT quantity_unit_pkey PRIMARY KEY (quantity_unit_id)
);


-- public.sale_cash_operator definition

-- Drop table

-- DROP TABLE public.sale_cash_operator;

CREATE TABLE public.sale_cash_operator (
	operator_id serial4 NOT NULL,
	op_num int4 DEFAULT 1 NOT NULL,
	printer_id int4 DEFAULT 1 NOT NULL,
	"name" varchar(100) NULL,
	pin varchar(10) DEFAULT '0000'::character varying NOT NULL,
	purse_id int4 DEFAULT 1 NOT NULL,
	CONSTRAINT sale_cash_operator_pkey PRIMARY KEY (operator_id)
);
CREATE INDEX sale_cash_operator_op_num ON public.sale_cash_operator USING btree (op_num);
CREATE INDEX sale_cash_operator_printer ON public.sale_cash_operator USING btree (printer_id);
CREATE INDEX sale_cash_operator_purse_id ON public.sale_cash_operator USING btree (purse_id);


-- public.sale_cash_printer definition

-- Drop table

-- DROP TABLE public.sale_cash_printer;

CREATE TABLE public.sale_cash_printer (
	printer_id serial4 NOT NULL,
	device_id int4 NULL,
	description varchar(80) NULL,
	host varchar(60) NULL,
	port int4 NULL,
	"type" int2 DEFAULT 2 NOT NULL,
	CONSTRAINT sale_cash_printer_pkey PRIMARY KEY (printer_id)
);
CREATE INDEX sale_cash_printer_descr ON public.sale_cash_printer USING btree (description);
CREATE INDEX sale_cash_printer_device_id_idx ON public.sale_cash_printer USING btree (device_id);
CREATE INDEX sale_cash_printer_host_idx ON public.sale_cash_printer USING btree (host);
CREATE INDEX sale_cash_printer_port_idx ON public.sale_cash_printer USING btree (port);
CREATE INDEX sale_cash_printer_type_idx ON public.sale_cash_printer USING btree (type);


-- public.sale_source definition

-- Drop table

-- DROP TABLE public.sale_source;

CREATE TABLE public.sale_source (
	source_id serial4 NOT NULL,
	description text NULL,
	CONSTRAINT sale_source_pkey PRIMARY KEY (source_id)
);
CREATE INDEX sale_source_description_idx ON public.sale_source USING btree (description);


-- public.sale_status definition

-- Drop table

-- DROP TABLE public.sale_status;

CREATE TABLE public.sale_status (
	sale_status_id smallserial NOT NULL,
	"name" varchar(30) DEFAULT ''::character varying NOT NULL,
	state int2 DEFAULT 0 NOT NULL,
	intl json NOT NULL,
	CONSTRAINT sale_status_pkey PRIMARY KEY (sale_status_id)
);
CREATE INDEX sale_status_name_idx ON public.sale_status USING btree (name);
CREATE INDEX sale_status_state_idx ON public.sale_status USING btree (state);


-- public.sale_unp_sequence definition

-- Drop table

-- DROP TABLE public.sale_unp_sequence;

CREATE TABLE public.sale_unp_sequence (
	serial varchar(60) NOT NULL,
	last_doc int4 NULL,
	CONSTRAINT sale_unp_sequence_pkey PRIMARY KEY (serial)
);
CREATE INDEX sale_unp_sequence_serial ON public.sale_unp_sequence USING btree (serial);


-- public.sun_settings definition

-- Drop table

-- DROP TABLE public.sun_settings;

CREATE TABLE public.sun_settings (
	"offset" int4 NOT NULL,
	city_id int4 NOT NULL,
	settings jsonb NULL,
	CONSTRAINT sun_settings_pkey PRIMARY KEY ("offset", city_id)
);
CREATE INDEX sun_settings_city_id ON public.sun_settings USING btree (city_id);
CREATE INDEX sun_settings_offset ON public.sun_settings USING btree ("offset");


-- public.supplier_group definition

-- Drop table

-- DROP TABLE public.supplier_group;

CREATE TABLE public.supplier_group (
	supplier_group_id serial4 NOT NULL,
	"name" varchar(60) NOT NULL,
	tax_client_id int2 DEFAULT 0 NOT NULL,
	CONSTRAINT supplier_group_pkey PRIMARY KEY (supplier_group_id)
);


-- public.support_attach definition

-- Drop table

-- DROP TABLE public.support_attach;

CREATE TABLE public.support_attach (
	attach_id serial4 NOT NULL,
	object_id int4 NOT NULL,
	to_type int4 DEFAULT 1 NOT NULL,
	"source" varchar(80) NULL,
	"comment" text NULL,
	"date" timestamptz NULL,
	creator_id int4 NOT NULL,
	CONSTRAINT support_attach_pkey PRIMARY KEY (attach_id)
);
CREATE INDEX support_attach_date ON public.support_attach USING btree (date);
CREATE INDEX support_attach_object_id ON public.support_attach USING btree (object_id);
CREATE INDEX support_attach_source ON public.support_attach USING btree (source);
CREATE INDEX support_attach_to_type ON public.support_attach USING btree (to_type);


-- public.support_card definition

-- Drop table

-- DROP TABLE public.support_card;

CREATE TABLE public.support_card (
	card_id int4 NOT NULL,
	card_version int2 NOT NULL,
	"name" varchar(50) NULL,
	user_id int4 NOT NULL,
	enabled bool DEFAULT true NOT NULL,
	CONSTRAINT support_card_pkey PRIMARY KEY (card_id)
);
CREATE INDEX support_card_card_version_idx ON public.support_card USING btree (card_version);
CREATE INDEX support_card_enabled_idx ON public.support_card USING btree (enabled);
CREATE INDEX support_card_name_idx ON public.support_card USING btree (name);
CREATE INDEX support_card_user_id_idx ON public.support_card USING btree (user_id);


-- public.support_cat definition

-- Drop table

-- DROP TABLE public.support_cat;

CREATE TABLE public.support_cat (
	cat_id serial4 NOT NULL,
	"name" varchar(130) NULL,
	hours_react int4 NOT NULL,
	group_ids jsonb DEFAULT '[]'::jsonb NOT NULL,
	CONSTRAINT support_cat_pkey PRIMARY KEY (cat_id)
);
CREATE INDEX support_cat_group_ids ON public.support_cat USING btree (group_ids);
CREATE INDEX support_cat_name ON public.support_cat USING btree (name);


-- public.support_message definition

-- Drop table

-- DROP TABLE public.support_message;

CREATE TABLE public.support_message (
	msg_id serial4 NOT NULL,
	ref_id int4 NULL,
	machine_id int4 NULL,
	state int2 DEFAULT 0 NOT NULL,
	title text NULL,
	message text NULL,
	stamp timestamptz DEFAULT now() NOT NULL,
	CONSTRAINT support_message_pkey PRIMARY KEY (msg_id)
);
CREATE INDEX support_message_ref_id ON public.support_message USING btree (ref_id);
CREATE INDEX support_message_stamp ON public.support_message USING btree (stamp);
CREATE INDEX support_message_state ON public.support_message USING btree (state);
CREATE INDEX support_message_title ON public.support_message USING btree (title);


-- public.support_notify definition

-- Drop table

-- DROP TABLE public.support_notify;

CREATE TABLE public.support_notify (
	not_id serial4 NOT NULL,
	"key" varchar(60) NOT NULL,
	user_id int4 NOT NULL,
	CONSTRAINT support_notify_pkey PRIMARY KEY (not_id)
);
CREATE INDEX support_notify_key_idx ON public.support_notify USING btree (key);
CREATE INDEX support_notify_user_id_idx ON public.support_notify USING btree (user_id);


-- public.support_parking_url definition

-- Drop table

-- DROP TABLE public.support_parking_url;

CREATE TABLE public.support_parking_url (
	url_id serial4 NOT NULL,
	"name" varchar(120) NOT NULL,
	url varchar(120) NOT NULL,
	machine_id int4 NULL,
	reservations bool DEFAULT false NOT NULL,
	rent bool DEFAULT true NOT NULL,
	hourly bool DEFAULT true NOT NULL,
	CONSTRAINT support_parking_url_pkey PRIMARY KEY (url_id)
);
CREATE INDEX support_parking_url_hourly_idx ON public.support_parking_url USING btree (hourly);
CREATE INDEX support_parking_url_machine_id_idx ON public.support_parking_url USING btree (machine_id);
CREATE INDEX support_parking_url_name_idx ON public.support_parking_url USING btree (name);
CREATE INDEX support_parking_url_rent_idx ON public.support_parking_url USING btree (rent);
CREATE INDEX support_parking_url_reservations_idx ON public.support_parking_url USING btree (reservations);
CREATE INDEX support_parking_url_url_idx ON public.support_parking_url USING btree (url);


-- public.support_plate definition

-- Drop table

-- DROP TABLE public.support_plate;

CREATE TABLE public.support_plate (
	plate_id serial4 NOT NULL,
	user_id int4 NOT NULL,
	registration varchar(40) NULL,
	enabled bool NULL,
	CONSTRAINT support_plate_pkey PRIMARY KEY (plate_id)
);
CREATE INDEX support_plate_enabled_idx ON public.support_plate USING btree (enabled);
CREATE INDEX support_plate_registration_idx ON public.support_plate USING btree (registration);

-- public.support_rem_user definition

-- Drop table

-- DROP TABLE public.support_rem_user;

CREATE TABLE public.support_rem_user (
	rem_id serial4 NOT NULL,
	machine_id int4 NOT NULL,
	user_id int4 NOT NULL,
	username varchar(60) NULL,
	"name" varchar(90) NULL,
	expires timestamptz NULL,
	active bool DEFAULT true NOT NULL,
	CONSTRAINT support_rem_user_pkey PRIMARY KEY (rem_id)
);
CREATE INDEX support_rem_user_active_idx ON public.support_rem_user USING btree (active);
CREATE INDEX support_rem_user_expires_idx ON public.support_rem_user USING btree (expires);
CREATE INDEX support_rem_user_machine_id_idx ON public.support_rem_user USING btree (machine_id);
CREATE INDEX support_rem_user_name_idx ON public.support_rem_user USING btree (name);
CREATE INDEX support_rem_user_user_id_idx ON public.support_rem_user USING btree (user_id);
CREATE INDEX support_rem_user_username_idx ON public.support_rem_user USING btree (username);


-- public.support_subscription_cat definition

-- Drop table

-- DROP TABLE public.support_subscription_cat;

CREATE TABLE public.support_subscription_cat (
	cat_id serial4 NOT NULL,
	credits_per_month numeric(12, 2) DEFAULT 0 NOT NULL,
	"name" varchar(90) NULL,
	settings jsonb NULL,
	CONSTRAINT support_subscription_cat_pkey PRIMARY KEY (cat_id)
);
CREATE INDEX support_subscription_cat_credits ON public.support_subscription_cat USING btree (credits_per_month);
CREATE INDEX support_subscription_cat_name ON public.support_subscription_cat USING btree (name);


-- public.support_w_type definition

-- Drop table

-- DROP TABLE public.support_w_type;

CREATE TABLE public.support_w_type (
	wt_id serial4 NOT NULL,
	"name" text NULL,
	CONSTRAINT support_w_type_pkey PRIMARY KEY (wt_id)
);
CREATE INDEX support_support_w_type_name ON public.support_w_type USING btree (name);


-- public.tax_client definition

-- Drop table

-- DROP TABLE public.tax_client;

CREATE TABLE public.tax_client (
	tax_client_id smallserial NOT NULL,
	"name" varchar(30) DEFAULT ''::character varying NOT NULL,
	CONSTRAINT tax_client_pkey PRIMARY KEY (tax_client_id)
);


-- public.tax_product definition

-- Drop table

-- DROP TABLE public.tax_product;

CREATE TABLE public.tax_product (
	tax_product_id smallserial NOT NULL,
	"name" varchar(30) DEFAULT ''::character varying NOT NULL,
	CONSTRAINT tax_product_pkey PRIMARY KEY (tax_product_id)
);


-- public.tax_rule definition

-- Drop table

-- DROP TABLE public.tax_rule;

CREATE TABLE public.tax_rule (
	tax_rule_id smallserial NOT NULL,
	"name" varchar(100) NOT NULL,
	tax_client jsonb NOT NULL,
	tax_product jsonb NOT NULL,
	tax_rate jsonb NOT NULL,
	use_origin_location bool DEFAULT false NOT NULL,
	tax_origin_location bool DEFAULT false NOT NULL,
	priority int2 DEFAULT 0 NOT NULL,
	"position" int2 DEFAULT 0 NOT NULL,
	active bool DEFAULT false NOT NULL,
	tax_only_on_vat_number bool DEFAULT false NOT NULL,
	CONSTRAINT tax_rule_pkey PRIMARY KEY (tax_rule_id)
);


-- public.user_group definition

-- Drop table

-- DROP TABLE public.user_group;

CREATE TABLE public.user_group (
	user_group_id smallserial NOT NULL,
	"name" varchar(100) NOT NULL,
	permissions jsonb NULL,
	reference varchar(120) NULL,
	idle_minutes int4 NULL,
	CONSTRAINT user_group_pkey PRIMARY KEY (user_group_id)
);
CREATE INDEX user_group_idle_minutes_idx ON public.user_group USING btree (idle_minutes);
CREATE INDEX user_group_name_idx ON public.user_group USING btree (name);
CREATE INDEX user_group_reference_idx ON public.user_group USING btree (reference);


-- public.warehouse_product_scrap_reason definition

-- Drop table

-- DROP TABLE public.warehouse_product_scrap_reason;

CREATE TABLE public.warehouse_product_scrap_reason (
	reason_id serial4 NOT NULL,
	"name" varchar(120) NULL,
	CONSTRAINT warehouse_product_scrap_reason_pkey PRIMARY KEY (reason_id)
);
CREATE INDEX warehouse_product_scrap_reason_name_idx ON public.warehouse_product_scrap_reason USING btree (name);


-- public.weight_unit definition

-- Drop table

-- DROP TABLE public.weight_unit;

CREATE TABLE public.weight_unit (
	weight_unit_id serial4 NOT NULL,
	unit varchar(30) DEFAULT ''::character varying NOT NULL,
	intl jsonb NOT NULL,
	value numeric(12, 5) DEFAULT 0 NOT NULL,
	CONSTRAINT weight_unit_pkey PRIMARY KEY (weight_unit_id)
);


-- public.xone_country definition

-- Drop table

-- DROP TABLE public.xone_country;

CREATE TABLE public.xone_country (
	country_id int8 NOT NULL,
	"name" varchar(120) NOT NULL,
	iso varchar(20) NOT NULL,
	"data" jsonb DEFAULT '{}'::jsonb NOT NULL,
	CONSTRAINT xone_country_pkey PRIMARY KEY (country_id)
);
CREATE INDEX xone_country_country_id_idx ON public.xone_country USING btree (country_id);
CREATE INDEX xone_country_iso_idx ON public.xone_country USING btree (iso);
CREATE INDEX xone_country_name_idx ON public.xone_country USING btree (name);


-- public.ac_cmd_reader_log definition

-- Drop table

-- DROP TABLE public.ac_cmd_reader_log;

CREATE TABLE public.ac_cmd_reader_log (
	log_id serial4 NOT NULL,
	reader_id int4 NOT NULL,
	stamp timestamptz DEFAULT now() NOT NULL,
	card_num int4 DEFAULT 0 NOT NULL,
	card_ver int4 DEFAULT 0 NOT NULL,
	"valid" bool DEFAULT true NOT NULL,
	CONSTRAINT ac_cmd_reader_log_pkey PRIMARY KEY (log_id),
	CONSTRAINT ac_cmd_reader_log_reader_id_fkey FOREIGN KEY (reader_id) REFERENCES public.ac_cmd_pi_reader(reader_id) ON DELETE RESTRICT
);
CREATE INDEX ac_cmd_reader_log_card_num_idx ON public.ac_cmd_reader_log USING btree (card_num);
CREATE INDEX ac_cmd_reader_log_card_ver_idx ON public.ac_cmd_reader_log USING btree (card_ver);
CREATE INDEX ac_cmd_reader_log_reader_id_idx ON public.ac_cmd_reader_log USING btree (reader_id);
CREATE INDEX ac_cmd_reader_log_stamp_idx ON public.ac_cmd_reader_log USING btree (stamp);
CREATE INDEX ac_cmd_reader_log_valid_idx ON public.ac_cmd_reader_log USING btree (valid);


-- public.ac_device_track_machine definition

-- Drop table

-- DROP TABLE public.ac_device_track_machine;

CREATE TABLE public.ac_device_track_machine (
	machine_id serial4 NOT NULL,
	machine_key varchar(130) NOT NULL,
	group_id int4 NULL,
	"name" text NULL,
	address varchar(30) NULL,
	customer_id int4 NULL,
	"comment" text NULL,
	support_data jsonb NULL,
	ssh_key text NULL,
	wgroup_id int4 NULL,
	files_size int8 DEFAULT '-1'::integer NOT NULL,
	base_size int8 DEFAULT '-1'::integer NOT NULL,
	max_files_size int8 DEFAULT 0 NOT NULL,
	max_base_size int8 DEFAULT 0 NOT NULL,
	modified timestamptz DEFAULT '1900-01-01 02:00:00+02'::timestamp with time zone NOT NULL,
	CONSTRAINT ac_device_track_machine_machine_key_key UNIQUE (machine_key),
	CONSTRAINT ac_device_track_machine_pkey PRIMARY KEY (machine_id),
	CONSTRAINT ac_device_track_machine_group_id_fkey FOREIGN KEY (group_id) REFERENCES public.ac_device_track_machine_group(group_id) ON DELETE RESTRICT,
	CONSTRAINT ac_device_track_machine_wgroup_id_fkey FOREIGN KEY (wgroup_id) REFERENCES public.ac_device_track_machine_wgroup(wgroup_id) ON DELETE RESTRICT
);
CREATE INDEX ac_device_track_machine_base_size ON public.ac_device_track_machine USING btree (base_size);
CREATE INDEX ac_device_track_machine_files_size ON public.ac_device_track_machine USING btree (files_size);
CREATE INDEX ac_device_track_machine_group_id ON public.ac_device_track_machine USING btree (group_id);
CREATE INDEX ac_device_track_machine_machine_id ON public.ac_device_track_machine USING btree (machine_key);
CREATE INDEX ac_device_track_machine_max_base_size ON public.ac_device_track_machine USING btree (max_base_size);
CREATE INDEX ac_device_track_machine_max_files_size ON public.ac_device_track_machine USING btree (max_files_size);
CREATE INDEX ac_device_track_machine_modified ON public.ac_device_track_machine USING btree (modified);
CREATE INDEX ac_device_track_machine_wgroup_id ON public.ac_device_track_machine USING btree (wgroup_id);


-- public.address definition

-- Drop table

-- DROP TABLE public.address;

CREATE TABLE public.address (
	address_id serial4 NOT NULL,
	first_name text NULL,
	last_name text NULL,
	phone varchar(60) NULL,
	country_id int4 NULL,
	country_zone_id int4 NULL,
	city text NULL,
	post varchar(30) NULL,
	address text NULL,
	address2 text NULL,
	CONSTRAINT address_pkey PRIMARY KEY (address_id),
	CONSTRAINT address_country_id_fkey FOREIGN KEY (country_id) REFERENCES public.country(country_id) ON DELETE RESTRICT,
	CONSTRAINT address_country_zone_id_fkey FOREIGN KEY (country_zone_id) REFERENCES public.country_zone(country_zone_id) ON DELETE RESTRICT
);


-- public.auth_sent_mail_att definition

-- Drop table

-- DROP TABLE public.auth_sent_mail_att;

CREATE TABLE public.auth_sent_mail_att (
	att_id bigserial NOT NULL,
	mail_id int4 NOT NULL,
	"source" text NULL,
	CONSTRAINT auth_sent_mail_att_pkey PRIMARY KEY (att_id),
	CONSTRAINT auth_sent_mail_att_mail_id_fkey FOREIGN KEY (mail_id) REFERENCES public.auth_sent_mail(mail_id) ON DELETE CASCADE
);
CREATE INDEX auth_sent_mail_att_mail_id_idx ON public.auth_sent_mail_att USING btree (mail_id);


-- public.auth_sent_mail_log definition

-- Drop table

-- DROP TABLE public.auth_sent_mail_log;

CREATE TABLE public.auth_sent_mail_log (
	log_id bigserial NOT NULL,
	mail_id int4 NOT NULL,
	ip varchar(40) NOT NULL,
	stamp timestamptz DEFAULT now() NOT NULL,
	CONSTRAINT auth_sent_mail_log_pkey PRIMARY KEY (log_id),
	CONSTRAINT auth_sent_mail_log_mail_id_fkey FOREIGN KEY (mail_id) REFERENCES public.auth_sent_mail(mail_id) ON DELETE CASCADE
);
CREATE INDEX auth_sent_mail_log_ip_idx ON public.auth_sent_mail_log USING btree (ip);
CREATE INDEX auth_sent_mail_log_mail_id_idx ON public.auth_sent_mail_log USING btree (mail_id);
CREATE INDEX auth_sent_mail_log_stamp_idx ON public.auth_sent_mail_log USING btree (stamp);


-- public.auth_sync_group definition

-- Drop table

-- DROP TABLE public.auth_sync_group;

CREATE TABLE public.auth_sync_group (
	group_id int4 NOT NULL,
	note varchar(120) NULL,
	machine_ids jsonb DEFAULT '[]'::jsonb NOT NULL,
	CONSTRAINT auth_sync_group_pkey PRIMARY KEY (group_id),
	CONSTRAINT auth_sync_group_group_id_fkey FOREIGN KEY (group_id) REFERENCES public.user_group(user_group_id) ON DELETE CASCADE
);
CREATE INDEX auth_sync_group_group_id_idx ON public.auth_sync_group USING btree (group_id);
CREATE INDEX auth_sync_group_note_idx ON public.auth_sync_group USING btree (note);


-- public.carrier_ship definition

-- Drop table

-- DROP TABLE public.carrier_ship;

CREATE TABLE public.carrier_ship (
	carrier_ship_id serial4 NOT NULL,
	carrier_id int4 NULL,
	bol varchar(30) NULL,
	"data" json NULL,
	CONSTRAINT carrier_ship_pkey PRIMARY KEY (carrier_ship_id),
	CONSTRAINT carrier_ship_carrier_id_fkey FOREIGN KEY (carrier_id) REFERENCES public.carrier(carrier_id) ON DELETE RESTRICT
);


-- public.case_doc_sub_type definition

-- Drop table

-- DROP TABLE public.case_doc_sub_type;

CREATE TABLE public.case_doc_sub_type (
	stype_id serial4 NOT NULL,
	type_id int4 NOT NULL,
	price numeric(12, 3) DEFAULT 0::numeric NOT NULL,
	creator_id int4 NOT NULL,
	"comment" text NULL,
	"date" timestamptz DEFAULT now() NOT NULL,
	"source" varchar(120) NULL,
	tax int2 DEFAULT '-1'::integer NOT NULL,
	price31 numeric(12, 3) DEFAULT 0::numeric NOT NULL,
	flags jsonb DEFAULT '{}'::jsonb NOT NULL,
	eflags jsonb DEFAULT '{}'::jsonb NOT NULL,
	dtypes jsonb DEFAULT '[]'::jsonb NOT NULL,
	doc_nr int4 DEFAULT 0 NOT NULL,
	CONSTRAINT case_doc_sub_type_pkey PRIMARY KEY (stype_id),
	CONSTRAINT case_doc_sub_type_type_id_fkey FOREIGN KEY (type_id) REFERENCES public.case_doc_type(type_id) ON DELETE RESTRICT
);
CREATE INDEX case_doc_sub_type_comment_idx ON public.case_doc_sub_type USING btree (comment);
CREATE INDEX case_doc_sub_type_creator_id_idx ON public.case_doc_sub_type USING btree (creator_id);
CREATE INDEX case_doc_sub_type_date_idx ON public.case_doc_sub_type USING btree (date);
CREATE INDEX case_doc_sub_type_doc_nr_idx ON public.case_doc_sub_type USING btree (doc_nr);
CREATE INDEX case_doc_sub_type_dtypes_idx ON public.case_doc_sub_type USING btree (dtypes);
CREATE INDEX case_doc_sub_type_eflags_idx ON public.case_doc_sub_type USING btree (eflags);
CREATE INDEX case_doc_sub_type_flags_idx ON public.case_doc_sub_type USING btree (flags);
CREATE INDEX case_doc_sub_type_source_idx ON public.case_doc_sub_type USING btree (source);
CREATE INDEX case_doc_sub_type_tax_idx ON public.case_doc_sub_type USING btree (tax);
CREATE INDEX case_doc_sub_type_type_id_idx ON public.case_doc_sub_type USING btree (type_id);


-- public.case_ind_active definition

-- Drop table

-- DROP TABLE public.case_ind_active;

CREATE TABLE public.case_ind_active (
	act_id serial4 NOT NULL,
	person_id int4 NOT NULL,
	nr varchar(60) NULL,
	type_id int2 DEFAULT 0 NOT NULL,
	created timestamptz DEFAULT now() NOT NULL,
	created_id int4 NOT NULL,
	modified timestamptz DEFAULT now() NOT NULL,
	modified_id int4 NOT NULL,
	actual bool DEFAULT true NOT NULL,
	CONSTRAINT case_ind_active_pkey PRIMARY KEY (act_id),
	CONSTRAINT case_ind_active_person_id_fkey FOREIGN KEY (person_id) REFERENCES public.case_person(person_id) ON DELETE RESTRICT
);
CREATE INDEX case_ind_active_created_id_idx ON public.case_ind_active USING btree (created_id);
CREATE INDEX case_ind_active_created_idx ON public.case_ind_active USING btree (created);
CREATE INDEX case_ind_active_modified_id_idx ON public.case_ind_active USING btree (modified_id);
CREATE INDEX case_ind_active_modified_idx ON public.case_ind_active USING btree (modified);
CREATE INDEX case_ind_active_nr_idx ON public.case_ind_active USING btree (nr);
CREATE INDEX case_ind_active_person_id_idx ON public.case_ind_active USING btree (person_id);
CREATE INDEX case_ind_active_type_id_idx ON public.case_ind_active USING btree (type_id);


-- public.case_jaddress definition

-- Drop table

-- DROP TABLE public.case_jaddress;

CREATE TABLE public.case_jaddress (
	ja_id bigserial NOT NULL,
	person_id int8 NOT NULL,
	stamp timestamptz DEFAULT now() NOT NULL,
	"data" jsonb DEFAULT '{}'::jsonb NOT NULL,
	data_bulstat jsonb DEFAULT '{}'::jsonb NOT NULL,
	CONSTRAINT case_jaddress_pkey PRIMARY KEY (ja_id),
	CONSTRAINT case_jaddress_person_id_fkey FOREIGN KEY (person_id) REFERENCES public.case_person(person_id) ON DELETE CASCADE
);
CREATE INDEX case_jaddress_person_id_idx ON public.case_jaddress USING btree (person_id);
CREATE INDEX case_jaddress_stamp_idx ON public.case_jaddress USING btree (stamp);


-- public.case_movable_actives definition

-- Drop table

-- DROP TABLE public.case_movable_actives;

CREATE TABLE public.case_movable_actives (
	act_id serial4 NOT NULL,
	person_id int4 NOT NULL,
	descr text NULL,
	created timestamptz DEFAULT now() NOT NULL,
	created_id int4 NOT NULL,
	modified timestamptz DEFAULT now() NOT NULL,
	modified_id int4 NOT NULL,
	actual bool DEFAULT true NOT NULL,
	CONSTRAINT case_movable_actives_pkey PRIMARY KEY (act_id),
	CONSTRAINT case_movable_actives_person_id_fkey FOREIGN KEY (person_id) REFERENCES public.case_person(person_id) ON DELETE RESTRICT
);
CREATE INDEX case_movable_actives_person_id_idx ON public.case_movable_actives USING btree (person_id);


-- public.case_ms_check definition

-- Drop table

-- DROP TABLE public.case_ms_check;

CREATE TABLE public.case_ms_check (
	chk_id bigserial NOT NULL,
	person_id int8 NOT NULL,
	"check" timestamptz DEFAULT now() NOT NULL,
	"data" jsonb DEFAULT '{}'::jsonb NOT NULL,
	CONSTRAINT case_ms_check_pkey PRIMARY KEY (chk_id),
	CONSTRAINT case_ms_check_person_id_fkey FOREIGN KEY (person_id) REFERENCES public.case_person(person_id) ON DELETE CASCADE
);
CREATE INDEX case_ms_check_check_idx ON public.case_ms_check USING btree ("check");
CREATE INDEX case_ms_check_person_id_idx ON public.case_ms_check USING btree (person_id);


-- public.case_paper_actives definition

-- Drop table

-- DROP TABLE public.case_paper_actives;

CREATE TABLE public.case_paper_actives (
	act_id serial4 NOT NULL,
	"type" int2 DEFAULT 0 NOT NULL,
	person_id int4 NOT NULL,
	company varchar(90) NULL,
	company_eik varchar(20) NULL,
	emission int4 DEFAULT 0 NOT NULL,
	count int4 DEFAULT 0 NOT NULL,
	nominal numeric(12, 3) DEFAULT 0 NOT NULL,
	created timestamptz DEFAULT now() NOT NULL,
	created_id int4 NOT NULL,
	modified timestamptz DEFAULT now() NOT NULL,
	modified_id int4 NOT NULL,
	actual bool DEFAULT true NOT NULL,
	CONSTRAINT case_paper_actives_pkey PRIMARY KEY (act_id),
	CONSTRAINT case_paper_actives_person_id_fkey FOREIGN KEY (person_id) REFERENCES public.case_person(person_id) ON DELETE RESTRICT
);
CREATE INDEX case_paper_actives_actual_idx ON public.case_paper_actives USING btree (actual);
CREATE INDEX case_paper_actives_created_id_idx ON public.case_paper_actives USING btree (created_id);
CREATE INDEX case_paper_actives_created_idx ON public.case_paper_actives USING btree (created);
CREATE INDEX case_paper_actives_modified_id_idx ON public.case_paper_actives USING btree (modified_id);
CREATE INDEX case_paper_actives_modified_idx ON public.case_paper_actives USING btree (modified);
CREATE INDEX case_paper_actives_person_id_idx ON public.case_paper_actives USING btree (person_id);
CREATE INDEX case_paper_actives_type_idx ON public.case_paper_actives USING btree (type);


-- public.case_part_of_factory definition

-- Drop table

-- DROP TABLE public.case_part_of_factory;

CREATE TABLE public.case_part_of_factory (
	act_id serial4 NOT NULL,
	person_id int4 NOT NULL,
	descr text NULL,
	created timestamptz DEFAULT now() NOT NULL,
	created_id int4 NOT NULL,
	modified timestamptz DEFAULT now() NOT NULL,
	modified_id int4 NOT NULL,
	actual bool DEFAULT true NOT NULL,
	CONSTRAINT case_part_of_factory_pkey PRIMARY KEY (act_id),
	CONSTRAINT case_part_of_factory_person_id_fkey FOREIGN KEY (person_id) REFERENCES public.case_person(person_id) ON DELETE RESTRICT
);
CREATE INDEX case_part_of_factory_created_id_idx ON public.case_part_of_factory USING btree (created_id);
CREATE INDEX case_part_of_factory_created_idx ON public.case_part_of_factory USING btree (created);
CREATE INDEX case_part_of_factory_modified_id_idx ON public.case_part_of_factory USING btree (modified_id);
CREATE INDEX case_part_of_factory_modified_idx ON public.case_part_of_factory USING btree (modified);
CREATE INDEX case_part_of_factory_person_id_idx ON public.case_part_of_factory USING btree (person_id);


-- public.case_pcontact definition

-- Drop table

-- DROP TABLE public.case_pcontact;

CREATE TABLE public.case_pcontact (
	cont_id bigserial NOT NULL,
	person_id int8 NOT NULL,
	stamp timestamptz DEFAULT now() NOT NULL,
	"name" varchar(120) NULL,
	"data" text NULL,
	"type" int2 DEFAULT '0'::smallint NOT NULL,
	active bool DEFAULT true NOT NULL,
	"comment" text NULL,
	CONSTRAINT case_pcontact_pkey PRIMARY KEY (cont_id),
	CONSTRAINT case_pcontact_person_id_fkey FOREIGN KEY (person_id) REFERENCES public.case_person(person_id) ON DELETE CASCADE
);
CREATE INDEX case_pcontact_active_idx ON public.case_pcontact USING btree (active);
CREATE INDEX case_pcontact_person_id_idx ON public.case_pcontact USING btree (person_id);
CREATE INDEX case_pcontact_stamp_idx ON public.case_pcontact USING btree (stamp);
CREATE INDEX case_pcontact_type_idx ON public.case_pcontact USING btree (type);


-- public.case_pension definition

-- Drop table

-- DROP TABLE public.case_pension;

CREATE TABLE public.case_pension (
	pid serial4 NOT NULL,
	person_id int4 NOT NULL,
	checked timestamptz DEFAULT now() NOT NULL,
	created_id int4 NULL,
	created timestamptz DEFAULT now() NOT NULL,
	title text NULL,
	div_noi text NULL,
	amount numeric(12, 3) DEFAULT 0 NOT NULL,
	currency_id int4 DEFAULT 1 NOT NULL,
	pensions jsonb DEFAULT '[]'::jsonb NOT NULL,
	modifiers jsonb DEFAULT '[]'::jsonb NOT NULL,
	"real" bool DEFAULT true NOT NULL,
	CONSTRAINT case_pension_pkey PRIMARY KEY (pid),
	CONSTRAINT case_pension_person_id_fkey FOREIGN KEY (person_id) REFERENCES public.case_person(person_id) ON DELETE CASCADE
);
CREATE INDEX case_pension_amount_idx ON public.case_pension USING btree (amount);
CREATE INDEX case_pension_checked_idx ON public.case_pension USING btree (checked);
CREATE INDEX case_pension_created_id_idx ON public.case_pension USING btree (created_id);
CREATE INDEX case_pension_created_idx ON public.case_pension USING btree (created);
CREATE INDEX case_pension_person_id_idx ON public.case_pension USING btree (person_id);
CREATE INDEX case_pension_real_idx ON public.case_pension USING btree ("real");


-- public.case_person_bank_account definition

-- Drop table

-- DROP TABLE public.case_person_bank_account;

CREATE TABLE public.case_person_bank_account (
	account_id serial4 NOT NULL,
	person_id int4 NOT NULL,
	iban varchar(40) NULL,
	bank_id int4 DEFAULT 0 NOT NULL,
	active bool DEFAULT true NOT NULL,
	CONSTRAINT case_person_bank_account_pkey PRIMARY KEY (account_id),
	CONSTRAINT case_person_bank_account_person_id_fkey FOREIGN KEY (person_id) REFERENCES public.case_person(person_id) ON DELETE CASCADE
);
CREATE INDEX case_person_bank_account_active_idx ON public.case_person_bank_account USING btree (active);
CREATE INDEX case_person_bank_account_iban_idx ON public.case_person_bank_account USING btree (iban);
CREATE INDEX case_person_bank_account_person_id_idx ON public.case_person_bank_account USING btree (person_id);


-- public.case_person_link definition

-- Drop table

-- DROP TABLE public.case_person_link;

CREATE TABLE public.case_person_link (
	link_id serial4 NOT NULL,
	from_person_id int4 NOT NULL,
	to_person_id int4 NOT NULL,
	"type" int2 DEFAULT 0 NOT NULL,
	CONSTRAINT case_person_link_pkey PRIMARY KEY (link_id),
	CONSTRAINT case_person_link_from_person_id_fkey FOREIGN KEY (from_person_id) REFERENCES public.case_person(person_id) ON DELETE CASCADE,
	CONSTRAINT case_person_link_to_person_id_fkey FOREIGN KEY (to_person_id) REFERENCES public.case_person(person_id) ON DELETE RESTRICT
);
CREATE INDEX case_person_link_from_person_id_idx ON public.case_person_link USING btree (from_person_id);
CREATE INDEX case_person_link_to_person_id_idx ON public.case_person_link USING btree (to_person_id);
CREATE INDEX case_person_link_type_idx ON public.case_person_link USING btree (type);


-- public.case_real_estate definition

-- Drop table

-- DROP TABLE public.case_real_estate;

CREATE TABLE public.case_real_estate (
	rs_id serial4 NOT NULL,
	person_id int4 NOT NULL,
	"name" varchar(90) NULL,
	from_id int2 DEFAULT 0::smallint NOT NULL,
	created timestamptz NULL,
	created_id int4 NOT NULL,
	modified timestamptz NULL,
	modified_id int4 NOT NULL,
	descr text NULL,
	actual bool DEFAULT true NOT NULL,
	"type" int2 DEFAULT 0 NOT NULL,
	CONSTRAINT case_real_estate_pkey PRIMARY KEY (rs_id),
	CONSTRAINT case_real_estate_person_id_fkey FOREIGN KEY (person_id) REFERENCES public.case_person(person_id) ON DELETE CASCADE
);
CREATE INDEX case_real_estate_actual_idx ON public.case_real_estate USING btree (actual);
CREATE INDEX case_real_estate_created_id_idx ON public.case_real_estate USING btree (created_id);
CREATE INDEX case_real_estate_created_id_idx1 ON public.case_real_estate USING btree (created_id);
CREATE INDEX case_real_estate_created_idx ON public.case_real_estate USING btree (created);
CREATE INDEX case_real_estate_created_idx1 ON public.case_real_estate USING btree (created);
CREATE INDEX case_real_estate_from_id_idx ON public.case_real_estate USING btree (from_id);
CREATE INDEX case_real_estate_modified_id_idx ON public.case_real_estate USING btree (modified_id);
CREATE INDEX case_real_estate_modified_id_idx1 ON public.case_real_estate USING btree (modified_id);
CREATE INDEX case_real_estate_modified_idx ON public.case_real_estate USING btree (modified);
CREATE INDEX case_real_estate_modified_idx1 ON public.case_real_estate USING btree (modified);
CREATE INDEX case_real_estate_name_idx ON public.case_real_estate USING btree (name);
CREATE INDEX case_real_estate_person_id_idx ON public.case_real_estate USING btree (person_id);
CREATE INDEX case_real_estate_type_idx ON public.case_real_estate USING btree (type);


-- public.case_regix_address definition

-- Drop table

-- DROP TABLE public.case_regix_address;

CREATE TABLE public.case_regix_address (
	ra_id serial4 NOT NULL,
	person_id int4 NOT NULL,
	stamp timestamptz DEFAULT now() NOT NULL,
	"from" timestamptz NOT NULL,
	district_code varchar(20) NULL,
	district_name varchar(120) NULL,
	mun_code varchar(20) NULL,
	mun_name varchar(120) NULL,
	town_code varchar(20) NULL,
	town_name varchar(120) NULL,
	loc_code varchar(20) NULL,
	loc_name varchar(120) NULL,
	build_num varchar(20) NULL,
	entrance varchar(20) NULL,
	floor int4 NULL,
	appartment varchar(20) NULL,
	permanent bool DEFAULT false NOT NULL,
	CONSTRAINT case_regix_address_pkey PRIMARY KEY (ra_id),
	CONSTRAINT case_regix_address_person_id_fkey FOREIGN KEY (person_id) REFERENCES public.case_person(person_id) ON DELETE CASCADE
);
CREATE INDEX case_regix_address_from_idx ON public.case_regix_address USING btree ("from");
CREATE INDEX case_regix_address_person_id_idx ON public.case_regix_address USING btree (person_id);
CREATE INDEX case_regix_address_stamp_idx ON public.case_regix_address USING btree (stamp);


-- public.case_regix_aircraft definition

-- Drop table

-- DROP TABLE public.case_regix_aircraft;

CREATE TABLE public.case_regix_aircraft (
	ra_id bigserial NOT NULL,
	person_id int8 NOT NULL,
	model varchar(120) NOT NULL,
	serial varchar(120) NOT NULL,
	"data" jsonb DEFAULT '{}'::jsonb NOT NULL,
	CONSTRAINT case_regix_aircraft_pkey PRIMARY KEY (ra_id),
	CONSTRAINT case_regix_aircraft_person_id_fkey FOREIGN KEY (person_id) REFERENCES public.case_person(person_id) ON DELETE CASCADE
);
CREATE INDEX case_regix_aircraft_model_idx ON public.case_regix_aircraft USING btree (model);
CREATE INDEX case_regix_aircraft_person_id_idx ON public.case_regix_aircraft USING btree (person_id);
CREATE INDEX case_regix_aircraft_serial_idx ON public.case_regix_aircraft USING btree (serial);


-- public.case_regix_real_estate definition

-- Drop table

-- DROP TABLE public.case_regix_real_estate;

CREATE TABLE public.case_regix_real_estate (
	re_id bigserial NOT NULL,
	person_id int8 NOT NULL,
	lot_num int4 DEFAULT 0 NOT NULL,
	"year" int4 DEFAULT 0 NOT NULL,
	"data" jsonb DEFAULT '{}'::jsonb NOT NULL,
	CONSTRAINT case_regix_real_estate_pkey PRIMARY KEY (re_id),
	CONSTRAINT case_regix_real_estate_person_id_fkey FOREIGN KEY (person_id) REFERENCES public.case_person(person_id) ON DELETE CASCADE
);
CREATE INDEX case_regix_real_estate_lot_num_idx ON public.case_regix_real_estate USING btree (lot_num);
CREATE INDEX case_regix_real_estate_person_id_idx ON public.case_regix_real_estate USING btree (person_id);
CREATE INDEX case_regix_real_estate_year_idx ON public.case_regix_real_estate USING btree (year);


-- public.case_rel_check definition

-- Drop table

-- DROP TABLE public.case_rel_check;

CREATE TABLE public.case_rel_check (
	chk_id bigserial NOT NULL,
	person_id int8 NOT NULL,
	"check" timestamptz DEFAULT now() NOT NULL,
	"data" jsonb DEFAULT '[]'::jsonb NOT NULL,
	CONSTRAINT case_rel_check_pkey PRIMARY KEY (chk_id),
	CONSTRAINT case_rel_check_person_id_fkey FOREIGN KEY (person_id) REFERENCES public.case_person(person_id) ON DELETE CASCADE
);
CREATE INDEX case_rel_check_check_idx ON public.case_rel_check USING btree ("check");
CREATE INDEX case_rel_check_person_id_idx ON public.case_rel_check USING btree (person_id);


-- public.case_rnames definition

-- Drop table

-- DROP TABLE public.case_rnames;

CREATE TABLE public.case_rnames (
	rn_id serial4 NOT NULL,
	person_id int4 NOT NULL,
	stamp timestamptz DEFAULT now() NOT NULL,
	"data" jsonb DEFAULT '{}'::jsonb NOT NULL,
	CONSTRAINT case_rnames_pkey PRIMARY KEY (rn_id),
	CONSTRAINT case_rnames_person_id_fkey FOREIGN KEY (person_id) REFERENCES public.case_person(person_id) ON DELETE CASCADE
);
CREATE INDEX case_rnames_data_idx ON public.case_rnames USING btree (data);
CREATE INDEX case_rnames_person_id_idx ON public.case_rnames USING btree (person_id);
CREATE INDEX case_rnames_stamp_idx ON public.case_rnames USING btree (stamp);


-- public.case_rnap_depth definition

-- Drop table

-- DROP TABLE public.case_rnap_depth;

CREATE TABLE public.case_rnap_depth (
	nd_id bigserial NOT NULL,
	person_id int8 NOT NULL,
	stamp timestamptz DEFAULT now() NOT NULL,
	"depth" bool DEFAULT false NOT NULL,
	CONSTRAINT case_rnap_depth_pkey PRIMARY KEY (nd_id),
	CONSTRAINT case_rnap_depth_person_id_fkey FOREIGN KEY (person_id) REFERENCES public.case_person(person_id) ON DELETE CASCADE
);
CREATE INDEX case_rnap_depth_depth_idx ON public.case_rnap_depth USING btree (depth);
CREATE INDEX case_rnap_depth_person_id_idx ON public.case_rnap_depth USING btree (person_id);
CREATE INDEX case_rnap_depth_stamp_idx ON public.case_rnap_depth USING btree (stamp);


-- public.case_tech_matter definition

-- Drop table

-- DROP TABLE public.case_tech_matter;

CREATE TABLE public.case_tech_matter (
	tech_id serial4 NOT NULL,
	person_id int4 NOT NULL,
	kind int2 DEFAULT 1 NOT NULL,
	type_id int4 NOT NULL,
	reg_nr varchar(20) NULL,
	descr text NULL,
	created timestamptz DEFAULT now() NOT NULL,
	created_id int4 NOT NULL,
	modified timestamptz DEFAULT now() NOT NULL,
	modified_id int4 NOT NULL,
	actual bool DEFAULT true NOT NULL,
	CONSTRAINT case_tech_matter_pkey PRIMARY KEY (tech_id),
	CONSTRAINT case_tech_matter_person_id_fkey FOREIGN KEY (person_id) REFERENCES public.case_person(person_id) ON DELETE RESTRICT,
	CONSTRAINT case_tech_matter_type_id_fkey FOREIGN KEY (type_id) REFERENCES public.case_tech_matter_type(type_id) ON DELETE RESTRICT
);
CREATE INDEX case_tech_matter_actual_idx ON public.case_tech_matter USING btree (actual);
CREATE INDEX case_tech_matter_actual_idx1 ON public.case_tech_matter USING btree (actual);
CREATE INDEX case_tech_matter_created_id_idx ON public.case_tech_matter USING btree (created_id);
CREATE INDEX case_tech_matter_created_id_idx1 ON public.case_tech_matter USING btree (created_id);
CREATE INDEX case_tech_matter_created_idx ON public.case_tech_matter USING btree (created);
CREATE INDEX case_tech_matter_created_idx1 ON public.case_tech_matter USING btree (created);
CREATE INDEX case_tech_matter_kind_idx ON public.case_tech_matter USING btree (kind);
CREATE INDEX case_tech_matter_modified_id_idx ON public.case_tech_matter USING btree (modified_id);
CREATE INDEX case_tech_matter_modified_id_idx1 ON public.case_tech_matter USING btree (modified_id);
CREATE INDEX case_tech_matter_modified_idx ON public.case_tech_matter USING btree (modified);
CREATE INDEX case_tech_matter_modified_idx1 ON public.case_tech_matter USING btree (modified);
CREATE INDEX case_tech_matter_person_id_idx ON public.case_tech_matter USING btree (person_id);
CREATE INDEX case_tech_matter_reg_nr_idx ON public.case_tech_matter USING btree (reg_nr);
CREATE INDEX case_tech_matter_type_id_idx ON public.case_tech_matter USING btree (type_id);


-- public.case_town definition

-- Drop table

-- DROP TABLE public.case_town;

CREATE TABLE public.case_town (
	town_id serial4 NOT NULL,
	country_id int4 NOT NULL,
	zone_id int4 NOT NULL,
	mun_id int4 NOT NULL,
	"name" varchar(120) NOT NULL,
	CONSTRAINT case_town_pkey PRIMARY KEY (town_id),
	CONSTRAINT case_town_mun_id_fkey FOREIGN KEY (mun_id) REFERENCES public.case_municipality(mun_id) ON DELETE RESTRICT
);
CREATE INDEX case_town_country_id_idx ON public.case_town USING btree (country_id);
CREATE INDEX case_town_mun_id_idx ON public.case_town USING btree (mun_id);
CREATE INDEX case_town_name_idx ON public.case_town USING btree (name);
CREATE INDEX case_town_zone_id_idx ON public.case_town USING btree (zone_id);


-- public.case_vhcl_model definition

-- Drop table

-- DROP TABLE public.case_vhcl_model;

CREATE TABLE public.case_vhcl_model (
	model_id serial4 NOT NULL,
	brand_id int4 NOT NULL,
	"name" varchar(120) NOT NULL,
	CONSTRAINT case_vhcl_model_pkey PRIMARY KEY (model_id),
	CONSTRAINT case_vhcl_model_brand_id_fkey FOREIGN KEY (brand_id) REFERENCES public.case_vhcl_brand(brand_id) ON DELETE RESTRICT
);
CREATE INDEX case_vhcl_model_brand_id_idx ON public.case_vhcl_model USING btree (brand_id);
CREATE INDEX case_vhcl_model_name_idx ON public.case_vhcl_model USING btree (name);


-- public.case_work_contract definition

-- Drop table

-- DROP TABLE public.case_work_contract;

CREATE TABLE public.case_work_contract (
	wc_id serial4 NOT NULL,
	person_id int4 NOT NULL,
	tstart timestamptz DEFAULT now() NOT NULL,
	last_amend timestamptz NULL,
	tend timestamptz NULL,
	reason int2 NULL,
	eco_code int4 NULL,
	remuneration int4 NULL,
	profession_name text NULL,
	profession_code int4 NULL,
	ekatte_code int4 NULL,
	full_name varchar(120) NULL,
	company_name text NULL,
	company_bulstat varchar(30) NULL,
	modified_id int4 NULL,
	modified timestamptz NULL,
	actual bool DEFAULT true NOT NULL,
	"real" bool DEFAULT true NOT NULL,
	CONSTRAINT case_work_contract_pkey PRIMARY KEY (wc_id),
	CONSTRAINT case_work_contract_person_id_fkey FOREIGN KEY (person_id) REFERENCES public.case_person(person_id) ON DELETE CASCADE
);
CREATE INDEX case_work_contract_actual_idx ON public.case_work_contract USING btree (actual);
CREATE INDEX case_work_contract_last_amend_idx ON public.case_work_contract USING btree (last_amend);
CREATE INDEX case_work_contract_modified_id_idx ON public.case_work_contract USING btree (modified_id);
CREATE INDEX case_work_contract_modified_idx ON public.case_work_contract USING btree (modified);
CREATE INDEX case_work_contract_person_id_idx ON public.case_work_contract USING btree (person_id);
CREATE INDEX case_work_contract_real_idx ON public.case_work_contract USING btree ("real");
CREATE INDEX case_work_contract_tend_idx ON public.case_work_contract USING btree (tend);
CREATE INDEX case_work_contract_tstart_idx ON public.case_work_contract USING btree (tstart);


-- public.cl_reg_event definition

-- Drop table

-- DROP TABLE public.cl_reg_event;

CREATE TABLE public.cl_reg_event (
	event_id serial4 NOT NULL,
	event_type_id int4 NOT NULL,
	document_id int4 NULL,
	tstamp timestamptz DEFAULT now() NOT NULL,
	customer_id int4 NULL,
	sale_id int4 NULL,
	"comment" text NULL,
	CONSTRAINT cl_reg_event_document_id_event_type_id_key UNIQUE (document_id, event_type_id),
	CONSTRAINT cl_reg_event_pkey PRIMARY KEY (event_id),
	CONSTRAINT cl_reg_event_event_type_id_fkey FOREIGN KEY (event_type_id) REFERENCES public.cl_reg_event_type(event_type_id) ON DELETE RESTRICT
);


-- public.cl_reg_record definition

-- Drop table

-- DROP TABLE public.cl_reg_record;

CREATE TABLE public.cl_reg_record (
	record_id serial4 NOT NULL,
	event_id int4 NOT NULL,
	tstamp timestamptz DEFAULT now() NOT NULL,
	attachment varchar(60) NULL,
	"comment" text NULL,
	link varchar(60) NULL,
	CONSTRAINT cl_reg_record_pkey PRIMARY KEY (record_id),
	CONSTRAINT cl_reg_record_event_id_fkey FOREIGN KEY (event_id) REFERENCES public.cl_reg_event(event_id) ON DELETE CASCADE
);


-- public.cl_salary_car_fuel definition

-- Drop table

-- DROP TABLE public.cl_salary_car_fuel;

CREATE TABLE public.cl_salary_car_fuel (
	fuel_id serial4 NOT NULL,
	car_id int4 NOT NULL,
	cur_km numeric(12, 2) NOT NULL,
	lt_left numeric(12, 2) NOT NULL,
	lt_fueled numeric(12, 2) NOT NULL,
	user_id int4 NOT NULL,
	stamp timestamptz DEFAULT now() NOT NULL,
	modified timestamptz DEFAULT now() NOT NULL,
	CONSTRAINT cl_salary_car_fuel_pkey PRIMARY KEY (fuel_id),
	CONSTRAINT cl_salary_car_fuel_car_id_fkey FOREIGN KEY (car_id) REFERENCES public.cl_salary_car(car_id) ON DELETE RESTRICT
);
CREATE INDEX cl_salary_car_fuel_car_id_idx ON public.cl_salary_car_fuel USING btree (car_id);
CREATE INDEX cl_salary_car_fuel_cur_km_idx ON public.cl_salary_car_fuel USING btree (cur_km);
CREATE INDEX cl_salary_car_fuel_lt_fueled_idx ON public.cl_salary_car_fuel USING btree (lt_fueled);
CREATE INDEX cl_salary_car_fuel_lt_left_idx ON public.cl_salary_car_fuel USING btree (lt_left);
CREATE INDEX cl_salary_car_fuel_modified_idx ON public.cl_salary_car_fuel USING btree (modified);
CREATE INDEX cl_salary_car_fuel_stamp_idx ON public.cl_salary_car_fuel USING btree (stamp);
CREATE INDEX cl_salary_car_fuel_user_id_idx ON public.cl_salary_car_fuel USING btree (user_id);


-- public.cl_salary_car_move definition

-- Drop table

-- DROP TABLE public.cl_salary_car_move;

CREATE TABLE public.cl_salary_car_move (
	move_id serial4 NOT NULL,
	car_id int4 NOT NULL,
	start_km numeric(12, 2) DEFAULT 0 NOT NULL,
	end_km numeric(12, 2) DEFAULT 0 NOT NULL,
	stamp timestamptz DEFAULT now() NOT NULL,
	"location" text NULL,
	user_id int4 NULL,
	modified timestamptz DEFAULT now() NOT NULL,
	CONSTRAINT cl_salary_car_move_pkey PRIMARY KEY (move_id),
	CONSTRAINT cl_salary_car_move_car_id_fkey FOREIGN KEY (car_id) REFERENCES public.cl_salary_car(car_id) ON DELETE RESTRICT
);
CREATE INDEX cl_salary_car_move_car_id_idx ON public.cl_salary_car_move USING btree (car_id);
CREATE INDEX cl_salary_car_move_end_km_idx ON public.cl_salary_car_move USING btree (end_km);
CREATE INDEX cl_salary_car_move_modified_idx ON public.cl_salary_car_move USING btree (modified);
CREATE INDEX cl_salary_car_move_stamp_idx ON public.cl_salary_car_move USING btree (stamp);
CREATE INDEX cl_salary_car_move_start_km_idx ON public.cl_salary_car_move USING btree (start_km);
CREATE INDEX cl_salary_car_move_user_id_idx ON public.cl_salary_car_move USING btree (user_id);


-- public.cl_salary_car_svc definition

-- Drop table

-- DROP TABLE public.cl_salary_car_svc;

CREATE TABLE public.cl_salary_car_svc (
	svc_id serial4 NOT NULL,
	car_id int4 NOT NULL,
	user_id int4 NOT NULL,
	notify_ids jsonb DEFAULT '[]'::jsonb NOT NULL,
	notify_evs jsonb DEFAULT '[]'::jsonb NOT NULL,
	stamp timestamptz DEFAULT now() NOT NULL,
	modified timestamptz DEFAULT now() NOT NULL,
	notify_after timestamptz NULL,
	descr varchar(90) NULL,
	CONSTRAINT cl_salary_car_svc_pkey PRIMARY KEY (svc_id),
	CONSTRAINT cl_salary_car_svc_car_id_fkey FOREIGN KEY (car_id) REFERENCES public.cl_salary_car(car_id) ON DELETE RESTRICT
);
CREATE INDEX cl_salary_car_svc_car_id_idx ON public.cl_salary_car_svc USING btree (car_id);
CREATE INDEX cl_salary_car_svc_descr_idx ON public.cl_salary_car_svc USING btree (descr);
CREATE INDEX cl_salary_car_svc_modified_idx ON public.cl_salary_car_svc USING btree (modified);
CREATE INDEX cl_salary_car_svc_notify_after_idx ON public.cl_salary_car_svc USING btree (notify_after);
CREATE INDEX cl_salary_car_svc_notify_ids_idx ON public.cl_salary_car_svc USING btree (notify_ids);
CREATE INDEX cl_salary_car_svc_stamp_idx ON public.cl_salary_car_svc USING btree (stamp);
CREATE INDEX cl_salary_car_svc_user_id_idx ON public.cl_salary_car_svc USING btree (user_id);


-- public.cl_salary_object definition

-- Drop table

-- DROP TABLE public.cl_salary_object;

CREATE TABLE public.cl_salary_object (
	obj_id serial4 NOT NULL,
	"name" varchar(90) NULL,
	machine_id int4 NULL,
	timezone text NULL,
	CONSTRAINT cl_salary_object_pkey PRIMARY KEY (obj_id),
	CONSTRAINT cl_salary_object_machine FOREIGN KEY (machine_id) REFERENCES public.auth_remote_machine(machine_id) ON DELETE RESTRICT
);
CREATE INDEX cl_salary_object_machine_id_idx ON public.cl_salary_object USING btree (machine_id);
CREATE INDEX cl_salary_object_name_idx ON public.cl_salary_object USING btree (name);
CREATE INDEX cl_salary_object_timezone_idx ON public.cl_salary_object USING btree (timezone);


-- public.cl_salary_worker_type definition

-- Drop table

-- DROP TABLE public.cl_salary_worker_type;

CREATE TABLE public.cl_salary_worker_type (
	type_id serial4 NOT NULL,
	descr varchar(80) NULL,
	schedule_id int4 NULL,
	CONSTRAINT cl_salary_worker_type_pkey PRIMARY KEY (type_id),
	CONSTRAINT cl_salary_worker_type_schedule_id_fkey FOREIGN KEY (schedule_id) REFERENCES public.personnel_group_schedule(schedule_id) ON DELETE RESTRICT
);
CREATE INDEX cl_salary_worker_type_descr ON public.cl_salary_worker_type USING btree (descr);
CREATE INDEX cl_salary_worker_type_schedule_id ON public.cl_salary_worker_type USING btree (schedule_id);


-- public.clt_task_attach definition

-- Drop table

-- DROP TABLE public.clt_task_attach;

CREATE TABLE public.clt_task_attach (
	attach_id serial4 NOT NULL,
	task_id int4 NOT NULL,
	"comment" text NULL,
	"source" varchar(100) NOT NULL,
	creator_id int4 NULL,
	"date" timestamptz NULL,
	CONSTRAINT clt_task_attach_pkey PRIMARY KEY (attach_id),
	CONSTRAINT clt_task_attach_task_id_fkey FOREIGN KEY (task_id) REFERENCES public.clt_task(task_id) ON DELETE CASCADE
);
CREATE INDEX clt_task_attach_task_id ON public.clt_task_attach USING btree (task_id);


-- public.clt_task_events definition

-- Drop table

-- DROP TABLE public.clt_task_events;

CREATE TABLE public.clt_task_events (
	event_id serial4 NOT NULL,
	task_id int4 NOT NULL,
	created timestamptz NOT NULL,
	creator_id int4 DEFAULT 1 NOT NULL,
	"comment" text NULL,
	address_id jsonb NULL,
	CONSTRAINT clt_task_events_pkey PRIMARY KEY (event_id),
	CONSTRAINT clt_task_events_task_id_fkey FOREIGN KEY (task_id) REFERENCES public.clt_task(task_id)
);
CREATE INDEX cl_task_events_task_id ON public.clt_task_events USING btree (task_id);


-- public.company definition

-- Drop table

-- DROP TABLE public.company;

CREATE TABLE public.company (
	company_id serial4 NOT NULL,
	"name" text NOT NULL,
	phone varchar(60) NULL,
	mail varchar(60) NULL,
	picture varchar(100) NULL,
	address_id int4 NULL,
	reg_address_id int4 NULL,
	uid_vat varchar(60) NULL,
	uid varchar(60) NULL,
	settings jsonb NULL,
	numbering int4 DEFAULT 0 NOT NULL,
	main int2 DEFAULT 0 NOT NULL,
	tsv tsvector NULL,
	zero_count int4 DEFAULT 10 NOT NULL,
	vat_reg jsonb DEFAULT '{}'::jsonb NOT NULL,
	CONSTRAINT company_pkey PRIMARY KEY (company_id),
	CONSTRAINT company_address_id_fkey FOREIGN KEY (address_id) REFERENCES public.address(address_id) ON DELETE SET NULL,
	CONSTRAINT company_reg_address_id_fkey FOREIGN KEY (reg_address_id) REFERENCES public.address(address_id) ON DELETE SET NULL
);
CREATE INDEX company_main_idx ON public.company USING btree (main);
CREATE INDEX company_name_idx ON public.company USING btree (name);
CREATE INDEX company_tsv_idx ON public.company USING btree (tsv);
CREATE INDEX company_uid_idx ON public.company USING btree (uid);
CREATE INDEX company_uid_vat_idx ON public.company USING btree (uid_vat);

-- Table Triggers

create trigger company_tsv_vector before
insert
    or
update
    on
    public.company for each row execute function company_tsv_vector();


-- public.company_attach definition

-- Drop table

-- DROP TABLE public.company_attach;

CREATE TABLE public.company_attach (
	company_attach_id serial4 NOT NULL,
	company_id int4 NOT NULL,
	"comment" text NULL,
	"source" varchar(100) NOT NULL,
	creator_id int4 NULL,
	"date" timestamptz NULL,
	CONSTRAINT company_attach_pkey PRIMARY KEY (company_attach_id),
	CONSTRAINT company_attach_company_id_fkey FOREIGN KEY (company_id) REFERENCES public.company(company_id) ON DELETE CASCADE
);
CREATE INDEX company_attach_company_id ON public.company_attach USING btree (company_id);
CREATE INDEX company_attach_date ON public.company_attach USING btree (date);


-- public.contr_cnotification_link definition

-- Drop table

-- DROP TABLE public.contr_cnotification_link;

CREATE TABLE public.contr_cnotification_link (
	cnot_id bigserial NOT NULL,
	not_id int8 NOT NULL,
	customer_id int4 NOT NULL,
	"read" timestamptz NULL,
	CONSTRAINT contr_cnotification_link_pkey PRIMARY KEY (cnot_id),
	CONSTRAINT contr_cnotification_link_not_id_fkey FOREIGN KEY (not_id) REFERENCES public.contr_cnotification(not_id) ON DELETE CASCADE
);
CREATE INDEX contr_cnotification_link_customer_id_idx ON public.contr_cnotification_link USING btree (customer_id);
CREATE INDEX contr_cnotification_link_not_id_idx ON public.contr_cnotification_link USING btree (not_id);
CREATE INDEX contr_cnotification_link_read_idx ON public.contr_cnotification_link USING btree (read);


-- public.country_loc definition

-- Drop table

-- DROP TABLE public.country_loc;

CREATE TABLE public.country_loc (
	country_id int4 NOT NULL,
	language_id int2 NOT NULL,
	"name" varchar(128) DEFAULT ''::character varying NOT NULL,
	CONSTRAINT country_loc_pkey PRIMARY KEY (country_id, language_id),
	CONSTRAINT country_loc_country_id_fkey FOREIGN KEY (country_id) REFERENCES public.country(country_id) ON DELETE CASCADE
);


-- public.country_zone_loc definition

-- Drop table

-- DROP TABLE public.country_zone_loc;

CREATE TABLE public.country_zone_loc (
	country_zone_id int4 NOT NULL,
	language_id int2 NOT NULL,
	"name" varchar(128) DEFAULT ''::character varying NOT NULL,
	CONSTRAINT country_zone_loc_pkey PRIMARY KEY (country_zone_id, language_id),
	CONSTRAINT country_zone_loc_country_zone_id_fkey FOREIGN KEY (country_zone_id) REFERENCES public.country_zone(country_zone_id) ON DELETE CASCADE
);


-- public.credit_pack_type definition

-- Drop table

-- DROP TABLE public.credit_pack_type;

CREATE TABLE public.credit_pack_type (
	pack_id serial4 NOT NULL,
	price numeric(12, 2) NULL,
	price_currency_id int2 NOT NULL,
	"name" varchar(90) NULL,
	expire_days int2 DEFAULT 30 NOT NULL,
	CONSTRAINT credit_pack_type_pkey PRIMARY KEY (pack_id),
	CONSTRAINT credit_pack_type_price_currency_id FOREIGN KEY (price_currency_id) REFERENCES public.currency(currency_id) ON DELETE RESTRICT
);
CREATE INDEX credit_pack_type_expire_days_idx ON public.credit_pack_type USING btree (expire_days);
CREATE INDEX credit_pack_type_name_idx ON public.credit_pack_type USING btree (name);
CREATE INDEX credit_pack_type_price_currency_id_idx ON public.credit_pack_type USING btree (price_currency_id);
CREATE INDEX credit_pack_type_price_idx ON public.credit_pack_type USING btree (price);


-- public.customer definition

-- Drop table

-- DROP TABLE public.customer;

CREATE TABLE public.customer (
	customer_id serial4 NOT NULL,
	active int2 DEFAULT 0 NOT NULL,
	first_name text NOT NULL,
	middle_name text NULL,
	last_name text NOT NULL,
	phone varchar(60) NULL,
	mail varchar(60) NULL,
	"password" varchar(60) NULL,
	picture varchar(100) NULL,
	pid varchar(20) NULL,
	pid_type int2 DEFAULT 0 NOT NULL,
	passport_nr varchar(20) NULL,
	passport_end_date timestamptz NULL,
	id_card_nr varchar(20) NULL,
	id_card_end_date timestamptz NULL,
	address_id int4 NULL,
	invoice_company text NULL,
	invoice_address_id int4 NULL,
	invoice_uid_vat varchar(60) NULL,
	invoice_uid varchar(60) NULL,
	date_created timestamptz DEFAULT now() NOT NULL,
	date_modified timestamptz DEFAULT now() NOT NULL,
	user_id int4 NULL,
	user_id_last int4 NULL,
	tsv tsvector NULL,
	notify_level int4 DEFAULT 2 NOT NULL,
	sms_notify bool DEFAULT false NOT NULL,
	language_id int4 DEFAULT 1 NOT NULL,
	currency_id int4 DEFAULT 1 NOT NULL,
	contactdata text NULL,
	gender int2 NULL,
	birthday timestamptz NULL,
	library_id int4 NULL,
	relations jsonb DEFAULT '{}'::jsonb NULL,
	cust_group_ids jsonb DEFAULT '[]'::jsonb NOT NULL,
	hash varchar(90) NULL,
	company_ids jsonb DEFAULT '[]'::jsonb NOT NULL,
	bevent_id int4 NULL,
	CONSTRAINT customer_pkey PRIMARY KEY (customer_id),
	CONSTRAINT customer_address_id_fkey FOREIGN KEY (address_id) REFERENCES public.address(address_id) ON DELETE SET NULL,
	CONSTRAINT customer_invoice_address_id_fkey FOREIGN KEY (invoice_address_id) REFERENCES public.address(address_id) ON DELETE SET NULL
);
CREATE INDEX customer_bevent_id_idx ON public.customer USING btree (bevent_id);
CREATE INDEX customer_birthday_idx ON public.customer USING btree (birthday);
CREATE INDEX customer_company_ids_idx ON public.customer USING btree (company_ids);
CREATE INDEX customer_cust_group_ids_idx ON public.customer USING btree (cust_group_ids);
CREATE INDEX customer_date_created_idx ON public.customer USING btree (date_created);
CREATE INDEX customer_date_modified_idx ON public.customer USING btree (date_modified);
CREATE INDEX customer_gender_idx ON public.customer USING btree (gender);
CREATE INDEX customer_hash_idx ON public.customer USING btree (hash);
CREATE INDEX customer_id_card_end_date_idx ON public.customer USING btree (id_card_end_date);
CREATE INDEX customer_id_card_nr_idx ON public.customer USING btree (id_card_nr);
CREATE INDEX customer_passport_end_date_idx ON public.customer USING btree (passport_end_date);
CREATE INDEX customer_passport_nr_idx ON public.customer USING btree (passport_nr);
CREATE INDEX customer_pid_idx ON public.customer USING btree (pid);
CREATE INDEX customer_pid_type_idx ON public.customer USING btree (pid_type);
CREATE INDEX customer_tsv_idx ON public.customer USING btree (tsv);
CREATE INDEX customer_user_id ON public.customer USING btree (user_id);
CREATE INDEX customer_user_id_last ON public.customer USING btree (user_id_last);

-- Table Triggers

create trigger customer_tsv_vector before
insert
    or
update
    on
    public.customer for each row execute function customer_tsv_vector();


-- public.customer_attach definition

-- Drop table

-- DROP TABLE public.customer_attach;

CREATE TABLE public.customer_attach (
	customer_attach_id serial4 NOT NULL,
	customer_id int4 NOT NULL,
	"comment" text NULL,
	"source" varchar(100) NOT NULL,
	creator_id int4 NULL,
	"date" timestamptz NULL,
	CONSTRAINT customer_attach_pkey PRIMARY KEY (customer_attach_id),
	CONSTRAINT customer_attach_customer_id_fkey FOREIGN KEY (customer_id) REFERENCES public.customer(customer_id) ON DELETE CASCADE
);
CREATE INDEX customer_attach_customer_id ON public.customer_attach USING btree (customer_id);
CREATE INDEX customer_attach_date ON public.customer_attach USING btree (date);


-- public.customer_balance definition

-- Drop table

-- DROP TABLE public.customer_balance;

CREATE TABLE public.customer_balance (
	customer_id int4 NOT NULL,
	currency_id int2 NOT NULL,
	amount numeric(12, 2) NOT NULL,
	CONSTRAINT customer_balance_currency_id_fkey FOREIGN KEY (currency_id) REFERENCES public.currency(currency_id) ON DELETE RESTRICT,
	CONSTRAINT customer_balance_customer_id_fkey FOREIGN KEY (customer_id) REFERENCES public.customer(customer_id) ON DELETE CASCADE
);


-- public.customer_transaction definition

-- Drop table

-- DROP TABLE public.customer_transaction;

CREATE TABLE public.customer_transaction (
	customer_transaction_id serial4 NOT NULL,
	customer_id int4 NOT NULL,
	currency_id int2 NOT NULL,
	amount numeric(12, 2) NOT NULL,
	"date" timestamptz DEFAULT now() NOT NULL,
	CONSTRAINT customer_transaction_pkey PRIMARY KEY (customer_transaction_id),
	CONSTRAINT customer_transaction_currency_id_fkey FOREIGN KEY (currency_id) REFERENCES public.currency(currency_id) ON DELETE RESTRICT,
	CONSTRAINT customer_transaction_customer_id_fkey FOREIGN KEY (customer_id) REFERENCES public.customer(customer_id) ON DELETE CASCADE
);

-- Table Triggers

create trigger customer_transaction_delete before
delete
    on
    public.customer_transaction for each row execute function customer_transaction_delete();
create trigger customer_transaction_insert after
insert
    on
    public.customer_transaction for each row execute function customer_transaction_insert();
create trigger customer_transaction_update after
update
    on
    public.customer_transaction for each row execute function customer_transaction_update();


-- public.inv_sequence definition

-- Drop table

-- DROP TABLE public.inv_sequence;

CREATE TABLE public.inv_sequence (
	seq_id serial4 NOT NULL,
	company_id int4 NOT NULL,
	zero_count int2 DEFAULT 0 NOT NULL,
	numbering int2 DEFAULT 0 NOT NULL,
	"name" varchar(90) NOT NULL,
	settings jsonb DEFAULT '{}'::jsonb NOT NULL,
	CONSTRAINT inv_sequence_pkey PRIMARY KEY (seq_id),
	CONSTRAINT inv_sequence_company_id_fkey FOREIGN KEY (company_id) REFERENCES public.company(company_id) ON DELETE CASCADE
);
CREATE INDEX inv_sequence_company_id_idx ON public.inv_sequence USING btree (company_id);


-- public.library_exported_fs definition

-- Drop table

-- DROP TABLE public.library_exported_fs;

CREATE TABLE public.library_exported_fs (
	export_id serial4 NOT NULL,
	"key" varchar(90) NULL,
	ip varchar(30) NULL,
	user_id int4 NOT NULL,
	entry_id int4 NOT NULL,
	CONSTRAINT library_exported_fs_pkey PRIMARY KEY (export_id),
	CONSTRAINT library_exported_fs_entry_id_fkey FOREIGN KEY (entry_id) REFERENCES public.library_file_entry(entry_id)
);
CREATE INDEX library_exported_fs_entry_id_idx ON public.library_exported_fs USING btree (entry_id);
CREATE INDEX library_exported_fs_ip_idx ON public.library_exported_fs USING btree (ip);
CREATE INDEX library_exported_fs_key_idx ON public.library_exported_fs USING btree (key);
CREATE INDEX library_exported_fs_user_id_idx ON public.library_exported_fs USING btree (user_id);


-- public.library_ext_sharing definition

-- Drop table

-- DROP TABLE public.library_ext_sharing;

CREATE TABLE public.library_ext_sharing (
	sharing_id serial4 NOT NULL,
	"key" varchar(100) NULL,
	entry_id int4 NOT NULL,
	stamp timestamptz NULL,
	keep_days int4 NULL,
	remove_type int2 DEFAULT 0 NOT NULL,
	CONSTRAINT library_ext_sharing_pkey PRIMARY KEY (sharing_id),
	CONSTRAINT library_ext_sharing_entry_id_fkey FOREIGN KEY (entry_id) REFERENCES public.library_file_entry(entry_id) ON DELETE CASCADE
);
CREATE INDEX library_ext_sharing_entry_id ON public.library_ext_sharing USING btree (entry_id);
CREATE INDEX library_ext_sharing_key ON public.library_ext_sharing USING btree (key);


-- public.library_root_path definition

-- Drop table

-- DROP TABLE public.library_root_path;

CREATE TABLE public.library_root_path (
	path_id serial4 NOT NULL,
	"path" varchar(100) NULL,
	link varchar(100) NULL,
	entry_id int4 NULL,
	CONSTRAINT library_root_path_pkey PRIMARY KEY (path_id),
	CONSTRAINT library_root_path_entry_id_fkey FOREIGN KEY (entry_id) REFERENCES public.library_file_entry(entry_id) ON DELETE CASCADE
);
CREATE INDEX library_root_path_entry_id ON public.library_root_path USING btree (entry_id);
CREATE INDEX library_root_path_link ON public.library_root_path USING btree (link);
CREATE INDEX library_root_path_path ON public.library_root_path USING btree (path);


-- public.personnel_group_report definition

-- Drop table

-- DROP TABLE public.personnel_group_report;

CREATE TABLE public.personnel_group_report (
	report_id serial4 NOT NULL,
	descr varchar(50) NULL,
	type_id int4 NOT NULL,
	tstart timestamptz NULL,
	tend timestamptz NULL,
	company_id int4 NOT NULL,
	address varchar(90) NULL,
	issuer varchar(90) NULL,
	CONSTRAINT personnel_group_report_pkey PRIMARY KEY (report_id),
	CONSTRAINT personnel_group_report_company FOREIGN KEY (company_id) REFERENCES public.company(company_id) ON DELETE RESTRICT
);
CREATE INDEX personnel_group_report_company_id ON public.personnel_group_report USING btree (company_id);
CREATE INDEX personnel_group_report_end ON public.personnel_group_report USING btree (tend);
CREATE INDEX personnel_group_report_schedule ON public.personnel_group_report USING btree (type_id);
CREATE INDEX personnel_group_report_start ON public.personnel_group_report USING btree (tstart);


-- public.product_attribute_data definition

-- Drop table

-- DROP TABLE public.product_attribute_data;

CREATE TABLE public.product_attribute_data (
	product_attribute_data_id serial4 NOT NULL,
	product_attribute_id int4 NOT NULL,
	"ref" varchar(100) NULL,
	title varchar(100) NOT NULL,
	"position" int4 DEFAULT 0 NOT NULL,
	CONSTRAINT product_attribute_data_pkey PRIMARY KEY (product_attribute_data_id),
	CONSTRAINT product_attribute_data_product_attribute_id_title_key UNIQUE (product_attribute_id, title),
	CONSTRAINT product_attribute_data_ref_key UNIQUE (ref),
	CONSTRAINT product_attribute_data_product_attribute_id_fkey FOREIGN KEY (product_attribute_id) REFERENCES public.product_attribute(product_attribute_id) ON DELETE CASCADE
);
CREATE INDEX product_attribute_data_product_attribute_id ON public.product_attribute_data USING btree (product_attribute_id);
CREATE INDEX product_attribute_data_ref ON public.product_attribute_data USING btree (ref);


-- public.product_attribute_data_loc definition

-- Drop table

-- DROP TABLE public.product_attribute_data_loc;

CREATE TABLE public.product_attribute_data_loc (
	product_attribute_data_id serial4 NOT NULL,
	language_id int2 NOT NULL,
	attribute_data_title varchar(100) NULL,
	CONSTRAINT product_attribute_data_loc_pkey PRIMARY KEY (product_attribute_data_id, language_id),
	CONSTRAINT product_attribute_data_loc_language_id_fkey FOREIGN KEY (language_id) REFERENCES public."language"(language_id) ON DELETE RESTRICT,
	CONSTRAINT product_attribute_data_loc_product_attribute_data_id_fkey FOREIGN KEY (product_attribute_data_id) REFERENCES public.product_attribute_data(product_attribute_data_id) ON DELETE CASCADE
);


-- public.product_attribute_loc definition

-- Drop table

-- DROP TABLE public.product_attribute_loc;

CREATE TABLE public.product_attribute_loc (
	product_attribute_id serial4 NOT NULL,
	language_id int2 NOT NULL,
	attribute_units varchar(10) NULL,
	attribute_title varchar(100) NULL,
	CONSTRAINT product_attribute_loc_pkey PRIMARY KEY (product_attribute_id, language_id),
	CONSTRAINT product_attribute_loc_language_id_fkey FOREIGN KEY (language_id) REFERENCES public."language"(language_id) ON DELETE RESTRICT,
	CONSTRAINT product_attribute_loc_product_attribute_id_fkey FOREIGN KEY (product_attribute_id) REFERENCES public.product_attribute(product_attribute_id) ON DELETE CASCADE
);


-- public.product_attribute_sub definition

-- Drop table

-- DROP TABLE public.product_attribute_sub;

CREATE TABLE public.product_attribute_sub (
	product_attribute_id int2 NOT NULL,
	product_attribute_sub_id int2 NOT NULL,
	"position" int2 DEFAULT 0 NOT NULL,
	CONSTRAINT product_attribute_sub_pkey PRIMARY KEY (product_attribute_id, product_attribute_sub_id),
	CONSTRAINT product_attribute_sub_product_attribute_id_fkey FOREIGN KEY (product_attribute_id) REFERENCES public.product_attribute(product_attribute_id) ON DELETE CASCADE,
	CONSTRAINT product_attribute_sub_product_attribute_sub_id_fkey FOREIGN KEY (product_attribute_sub_id) REFERENCES public.product_attribute(product_attribute_id) ON DELETE CASCADE
);
CREATE INDEX product_attribute_sub_product_attribute_sub_id ON public.product_attribute_sub USING btree (product_attribute_sub_id);


-- public.product_group_filter definition

-- Drop table

-- DROP TABLE public.product_group_filter;

CREATE TABLE public.product_group_filter (
	product_group_id int4 NOT NULL,
	product_attribute_id int4 DEFAULT 0 NOT NULL,
	"filter" int4 NULL,
	"position" int4 DEFAULT 0 NOT NULL,
	CONSTRAINT product_group_filter_pkey PRIMARY KEY (product_group_id, product_attribute_id),
	CONSTRAINT product_group_filter_product_group_id_fkey FOREIGN KEY (product_group_id) REFERENCES public.product_group(product_group_id) ON DELETE CASCADE
);
CREATE INDEX product_group_filter_product_group_id ON public.product_group_filter USING btree (product_group_id);


-- public.product_group_multimedia definition

-- Drop table

-- DROP TABLE public.product_group_multimedia;

CREATE TABLE public.product_group_multimedia (
	product_group_multimedia_id serial4 NOT NULL,
	product_group_id int4 NOT NULL,
	"type" int2 DEFAULT 0 NOT NULL,
	main int2 DEFAULT 0 NOT NULL,
	"source" varchar(100) NULL,
	alt varchar(100) NULL,
	CONSTRAINT product_group_multimedia_pkey PRIMARY KEY (product_group_multimedia_id),
	CONSTRAINT product_group_multimedia_product_group_id_fkey FOREIGN KEY (product_group_id) REFERENCES public.product_group(product_group_id) ON DELETE CASCADE
);
CREATE INDEX product_group_multimedia_product_group_id ON public.product_group_multimedia USING btree (product_group_id);


-- public.product_group_stats definition

-- Drop table

-- DROP TABLE public.product_group_stats;

CREATE TABLE public.product_group_stats (
	product_group_id int4 NOT NULL,
	viewed int4 DEFAULT 0 NOT NULL,
	CONSTRAINT product_group_stats_pkey PRIMARY KEY (product_group_id),
	CONSTRAINT product_group_stats_product_group_id_fkey FOREIGN KEY (product_group_id) REFERENCES public.product_group(product_group_id) ON DELETE CASCADE
);


-- public.product_group_web definition

-- Drop table

-- DROP TABLE public.product_group_web;

CREATE TABLE public.product_group_web (
	product_group_id int4 NOT NULL,
	language_id int2 NOT NULL,
	group_title varchar(100) NULL,
	web_url varchar(100) NULL,
	web_title varchar(100) NULL,
	web_keywords varchar(255) NULL,
	web_description varchar(255) NULL,
	"content" text NULL,
	CONSTRAINT product_group_web_pkey PRIMARY KEY (product_group_id, language_id),
	CONSTRAINT product_group_web_language_id_fkey FOREIGN KEY (language_id) REFERENCES public."language"(language_id) ON DELETE RESTRICT,
	CONSTRAINT product_group_web_product_group_id_fkey FOREIGN KEY (product_group_id) REFERENCES public.product_group(product_group_id) ON DELETE CASCADE
);


-- public.product_type definition

-- Drop table

-- DROP TABLE public.product_type;

CREATE TABLE public.product_type (
	product_type_id smallserial NOT NULL,
	title varchar(100) NOT NULL,
	"ref" varchar(100) NULL,
	product_group_id int4 NULL,
	price1_currency_id int4 NULL,
	price2_currency_id int4 NULL,
	tax1_product_id int2 NULL,
	tax2_product_id int2 NULL,
	quantity_unit_id int2 NULL,
	weight_unit_id int2 NULL,
	serial_type int2 NULL,
	"type" int2 NOT NULL,
	CONSTRAINT product_type_pkey PRIMARY KEY (product_type_id),
	CONSTRAINT product_type_ref_key UNIQUE (ref),
	CONSTRAINT product_type_title_key UNIQUE (title),
	CONSTRAINT product_type_price1_currency_id_fkey FOREIGN KEY (price1_currency_id) REFERENCES public.currency(currency_id) ON DELETE SET NULL,
	CONSTRAINT product_type_price2_currency_id_fkey FOREIGN KEY (price2_currency_id) REFERENCES public.currency(currency_id) ON DELETE SET NULL,
	CONSTRAINT product_type_product_group_id_fkey FOREIGN KEY (product_group_id) REFERENCES public.product_group(product_group_id) ON DELETE SET NULL,
	CONSTRAINT product_type_quantity_unit_id_fkey FOREIGN KEY (quantity_unit_id) REFERENCES public.quantity_unit(quantity_unit_id) ON DELETE SET NULL,
	CONSTRAINT product_type_weight_unit_id_fkey FOREIGN KEY (weight_unit_id) REFERENCES public.weight_unit(weight_unit_id) ON DELETE SET NULL
);
CREATE INDEX product_type_ref ON public.product_type USING btree (ref);


-- public.product_type_data definition

-- Drop table

-- DROP TABLE public.product_type_data;

CREATE TABLE public.product_type_data (
	product_type_id int2 NOT NULL,
	product_attribute_id int4 NOT NULL,
	"position" int4 DEFAULT 0 NOT NULL,
	CONSTRAINT product_type_data_pkey PRIMARY KEY (product_type_id, product_attribute_id),
	CONSTRAINT product_type_data_product_attribute_id_fkey FOREIGN KEY (product_attribute_id) REFERENCES public.product_attribute(product_attribute_id) ON DELETE CASCADE,
	CONSTRAINT product_type_data_product_type_id_fkey FOREIGN KEY (product_type_id) REFERENCES public.product_type(product_type_id) ON DELETE CASCADE
);


-- public.product_type_price definition

-- Drop table

-- DROP TABLE public.product_type_price;

CREATE TABLE public.product_type_price (
	product_type_price_id serial4 NOT NULL,
	product_type_id int2 NOT NULL,
	customer_group_id int4 NOT NULL,
	modification numeric(12, 2) DEFAULT 0 NOT NULL,
	CONSTRAINT product_type_price_pkey PRIMARY KEY (product_type_price_id),
	CONSTRAINT product_type_price_product_type_id_customer_group_id_key UNIQUE (product_type_id, customer_group_id),
	CONSTRAINT product_type_price_product_type_id_fkey FOREIGN KEY (product_type_id) REFERENCES public.product_type(product_type_id) ON DELETE CASCADE
);


-- public.promo_coupon_history definition

-- Drop table

-- DROP TABLE public.promo_coupon_history;

CREATE TABLE public.promo_coupon_history (
	coupon_id int4 NOT NULL,
	sale_id int4 NOT NULL,
	CONSTRAINT promo_coupon_history_coupon_id_fkey FOREIGN KEY (coupon_id) REFERENCES public.promo_coupon(promo_coupon_id) ON DELETE CASCADE
);
CREATE INDEX coupon_id_promo_coupon_history ON public.promo_coupon_history USING btree (coupon_id);


-- public.purchase_status_reason definition

-- Drop table

-- DROP TABLE public.purchase_status_reason;

CREATE TABLE public.purchase_status_reason (
	reason_id serial4 NOT NULL,
	status_id int4 NOT NULL,
	"name" varchar(60) NULL,
	CONSTRAINT purchase_status_reason_pkey PRIMARY KEY (reason_id),
	CONSTRAINT purchase_status_reason_status_id_fkey FOREIGN KEY (status_id) REFERENCES public.purchase_status(status_id)
);
CREATE INDEX purchase_status_reason_status ON public.purchase_status_reason USING btree (status_id);


-- public.purse_mod definition

-- Drop table

-- DROP TABLE public.purse_mod;

CREATE TABLE public.purse_mod (
	mod_id bigserial NOT NULL,
	purse_id int4 NOT NULL,
	stamp timestamptz DEFAULT now() NOT NULL,
	modified_id int4 NOT NULL,
	old_rec jsonb DEFAULT '{}'::jsonb NOT NULL,
	new_rec jsonb DEFAULT '{}'::jsonb NOT NULL,
	CONSTRAINT purse_mod_pkey PRIMARY KEY (mod_id),
	CONSTRAINT purse_mod_purse_id_fkey FOREIGN KEY (purse_id) REFERENCES public.purse(purse_id) ON DELETE CASCADE
);
CREATE INDEX purse_mod_modified_id_idx ON public.purse_mod USING btree (modified_id);
CREATE INDEX purse_mod_purse_id_idx ON public.purse_mod USING btree (purse_id);
CREATE INDEX purse_mod_stamp_idx ON public.purse_mod USING btree (stamp);


-- public.purse_out_money definition

-- Drop table

-- DROP TABLE public.purse_out_money;

CREATE TABLE public.purse_out_money (
	transaction_id int8 NOT NULL,
	to_bank_id int2 DEFAULT '-1'::integer NOT NULL,
	to_iban varchar(120) NULL,
	from_bank_id int2 DEFAULT '-1'::integer NOT NULL,
	from_iban varchar(120) NULL,
	sent timestamptz NULL,
	sent_id int4 NULL,
	reason text NULL,
	add_data jsonb DEFAULT '{}'::jsonb NOT NULL,
	CONSTRAINT purse_out_money_transaction_id_fkey FOREIGN KEY (transaction_id) REFERENCES public.purse_transaction(transaction_id) ON DELETE CASCADE
);
CREATE INDEX purse_out_money_from_bank_id_idx ON public.purse_out_money USING btree (from_bank_id);
CREATE INDEX purse_out_money_from_iban_idx ON public.purse_out_money USING btree (from_iban);
CREATE INDEX purse_out_money_sent_id_idx ON public.purse_out_money USING btree (sent_id);
CREATE INDEX purse_out_money_sent_idx ON public.purse_out_money USING btree (sent);
CREATE INDEX purse_out_money_to_bank_id_idx ON public.purse_out_money USING btree (to_bank_id);
CREATE INDEX purse_out_money_to_iban_idx ON public.purse_out_money USING btree (to_iban);
CREATE INDEX purse_out_money_transaction_id_idx ON public.purse_out_money USING btree (transaction_id);


-- public.sale_return definition

-- Drop table

-- DROP TABLE public.sale_return;

CREATE TABLE public.sale_return (
	sale_return_id serial4 NOT NULL,
	customer_id int4 NOT NULL,
	"date" timestamptz DEFAULT now() NOT NULL,
	CONSTRAINT sale_return_pkey PRIMARY KEY (sale_return_id),
	CONSTRAINT sale_return_customer_id_fkey FOREIGN KEY (customer_id) REFERENCES public.customer(customer_id) ON DELETE RESTRICT
);
CREATE INDEX sale_return_customer_id ON public.sale_return USING btree (customer_id);


-- public.sale_ro_corrector definition

-- Drop table

-- DROP TABLE public.sale_ro_corrector;

CREATE TABLE public.sale_ro_corrector (
	roc_id serial4 NOT NULL,
	"name" varchar(120) NOT NULL,
	"percent" numeric(12, 3) DEFAULT 0 NOT NULL,
	customer_id int4 NULL,
	CONSTRAINT sale_ro_corrector_pkey PRIMARY KEY (roc_id),
	CONSTRAINT sale_ro_corrector_customer_id_fkey FOREIGN KEY (customer_id) REFERENCES public.customer(customer_id) ON DELETE CASCADE
);
CREATE INDEX sale_ro_corrector_name_idx ON public.sale_ro_corrector USING btree (name);


-- public.sale_status_reason definition

-- Drop table

-- DROP TABLE public.sale_status_reason;

CREATE TABLE public.sale_status_reason (
	reason_id serial4 NOT NULL,
	state int2 NULL,
	status_id int2 NOT NULL,
	"name" text NULL,
	CONSTRAINT sale_status_reason_pkey PRIMARY KEY (reason_id),
	CONSTRAINT sale_status_reason_status_id_fkey FOREIGN KEY (status_id) REFERENCES public.sale_status(sale_status_id) ON DELETE RESTRICT
);
CREATE INDEX sale_status_reason_state ON public.sale_status_reason USING btree (state);
CREATE INDEX sale_status_reason_status_id ON public.sale_status_reason USING btree (status_id);


-- public.supplier definition

-- Drop table

-- DROP TABLE public.supplier;

CREATE TABLE public.supplier (
	supplier_id serial4 NOT NULL,
	active int2 DEFAULT 0 NOT NULL,
	first_name text NOT NULL,
	middle_name text NULL,
	last_name text NOT NULL,
	phone varchar(60) NULL,
	mail varchar(60) NULL,
	picture varchar(100) NULL,
	invoice_company text NULL,
	invoice_address_id int4 NULL,
	invoice_uid_vat varchar(60) NULL,
	invoice_uid varchar(60) NULL,
	date_created timestamptz DEFAULT now() NOT NULL,
	date_modified timestamptz DEFAULT now() NOT NULL,
	user_id int4 NULL,
	user_id_last int4 NULL,
	tsv tsvector NULL,
	library_id int4 NULL,
	contactdata text NULL,
	supp_group_ids jsonb DEFAULT '[]'::jsonb NOT NULL,
	company_ids jsonb DEFAULT '[]'::jsonb NOT NULL,
	vat_reg jsonb DEFAULT '{}'::jsonb NOT NULL,
	birthday timestamptz NULL,
	bevent_id int4 NULL,
	language_id int2 NULL,
	CONSTRAINT supplier_pkey PRIMARY KEY (supplier_id),
	CONSTRAINT supplier_invoice_address_id_fkey FOREIGN KEY (invoice_address_id) REFERENCES public.address(address_id) ON DELETE SET NULL
);
CREATE INDEX supplier_bevent_id_idx ON public.supplier USING btree (bevent_id);
CREATE INDEX supplier_birthday_idx ON public.supplier USING btree (birthday);
CREATE INDEX supplier_company_ids_idx ON public.supplier USING btree (company_ids);
CREATE INDEX supplier_language_id_idx ON public.supplier USING btree (language_id);
CREATE INDEX supplier_supp_group_ids_idx ON public.supplier USING btree (supp_group_ids);
CREATE INDEX supplier_tsv ON public.supplier USING btree (tsv);
CREATE INDEX supplier_user_id ON public.supplier USING btree (user_id);
CREATE INDEX supplier_user_id_last ON public.supplier USING btree (user_id_last);

-- Table Triggers

create trigger supplier_tsv_vector before
insert
    or
update
    on
    public.supplier for each row execute function supplier_tsv_vector();


-- public.supplier_attach definition

-- Drop table

-- DROP TABLE public.supplier_attach;

CREATE TABLE public.supplier_attach (
	supplier_attach_id serial4 NOT NULL,
	supplier_id int4 NOT NULL,
	"comment" text NULL,
	"source" varchar(100) NOT NULL,
	CONSTRAINT supplier_attach_pkey PRIMARY KEY (supplier_attach_id),
	CONSTRAINT supplier_attach_supplier_id_fkey FOREIGN KEY (supplier_id) REFERENCES public.supplier(supplier_id) ON DELETE CASCADE
);
CREATE INDEX supplier_attach_supplier_id ON public.supplier_attach USING btree (supplier_id);


-- public.supplier_balance definition

-- Drop table

-- DROP TABLE public.supplier_balance;

CREATE TABLE public.supplier_balance (
	supplier_id int4 NOT NULL,
	currency_id int2 NOT NULL,
	amount numeric(12, 2) NOT NULL,
	CONSTRAINT supplier_balance_currency_id_fkey FOREIGN KEY (currency_id) REFERENCES public.currency(currency_id) ON DELETE RESTRICT,
	CONSTRAINT supplier_balance_supplier_id_fkey FOREIGN KEY (supplier_id) REFERENCES public.supplier(supplier_id) ON DELETE CASCADE
);


-- public.supplier_transaction definition

-- Drop table

-- DROP TABLE public.supplier_transaction;

CREATE TABLE public.supplier_transaction (
	supplier_transaction_id serial4 NOT NULL,
	supplier_id int4 NOT NULL,
	currency_id int2 NOT NULL,
	amount numeric(12, 2) NOT NULL,
	"date" timestamptz DEFAULT now() NOT NULL,
	CONSTRAINT supplier_transaction_pkey PRIMARY KEY (supplier_transaction_id),
	CONSTRAINT supplier_transaction_currency_id_fkey FOREIGN KEY (currency_id) REFERENCES public.currency(currency_id) ON DELETE RESTRICT,
	CONSTRAINT supplier_transaction_supplier_id_fkey FOREIGN KEY (supplier_id) REFERENCES public.supplier(supplier_id) ON DELETE CASCADE
);

-- Table Triggers

create trigger supplier_transaction_delete before
delete
    on
    public.supplier_transaction for each row execute function supplier_transaction_delete();
create trigger supplier_transaction_insert after
insert
    on
    public.supplier_transaction for each row execute function supplier_transaction_insert();
create trigger supplier_transaction_update after
update
    on
    public.supplier_transaction for each row execute function supplier_transaction_update();


-- public.support_subscription definition

-- Drop table

-- DROP TABLE public.support_subscription;

CREATE TABLE public.support_subscription (
	subscription_id serial4 NOT NULL,
	cat_id int4 NOT NULL,
	machine_id int4 NOT NULL,
	renew_date int4 NULL,
	renewed_at timestamptz NULL,
	CONSTRAINT support_subscription_cat_id_fkey FOREIGN KEY (cat_id) REFERENCES public.support_subscription_cat(cat_id) ON DELETE RESTRICT
);
CREATE INDEX support_subscription_cat_id ON public.support_subscription USING btree (cat_id);
CREATE INDEX support_subscription_machine_id ON public.support_subscription USING btree (machine_id);
CREATE INDEX support_subscription_renew_date ON public.support_subscription USING btree (renew_date);
CREATE INDEX support_subscription_renewed_at ON public.support_subscription USING btree (renewed_at);


-- public.support_type definition

-- Drop table

-- DROP TABLE public.support_type;

CREATE TABLE public.support_type (
	type_id serial4 NOT NULL,
	cat_id int4 NOT NULL,
	"name" text NULL,
	hours_work int4 NOT NULL,
	CONSTRAINT support_type_pkey PRIMARY KEY (type_id),
	CONSTRAINT support_type_cat_id_fkey FOREIGN KEY (cat_id) REFERENCES public.support_cat(cat_id) ON DELETE RESTRICT
);
CREATE INDEX support_type_cat_id ON public.support_type USING btree (cat_id);
CREATE INDEX support_type_name ON public.support_type USING btree (name);


-- public.tax_rate definition

-- Drop table

-- DROP TABLE public.tax_rate;

CREATE TABLE public.tax_rate (
	tax_rate_id smallserial NOT NULL,
	"name" varchar(100) DEFAULT ''::character varying NOT NULL,
	intl jsonb NOT NULL,
	rate numeric(12, 5) DEFAULT 0 NOT NULL,
	country_id int4 NOT NULL,
	country_zone_id int4 NULL,
	CONSTRAINT tax_rate_pkey PRIMARY KEY (tax_rate_id),
	CONSTRAINT tax_rate_country_id_fkey FOREIGN KEY (country_id) REFERENCES public.country(country_id) ON DELETE RESTRICT,
	CONSTRAINT tax_rate_country_zone_id_fkey FOREIGN KEY (country_zone_id) REFERENCES public.country_zone(country_zone_id) ON DELETE RESTRICT
);


-- public."user" definition

-- Drop table

-- DROP TABLE public."user";

CREATE TABLE public."user" (
	user_id smallserial NOT NULL,
	user_group_id int2 NOT NULL,
	username varchar(100) NOT NULL,
	"password" varchar(60) NULL,
	"name" varchar(100) NULL,
	mail varchar(100) NULL,
	active bool DEFAULT false NOT NULL,
	picture varchar(100) NULL,
	settings jsonb NOT NULL,
	hidden bool DEFAULT false NOT NULL,
	expires timestamptz NULL,
	ref_id int8 NULL,
	"comment" text NULL,
	CONSTRAINT user_pkey PRIMARY KEY (user_id),
	CONSTRAINT user_user_group_id_fkey FOREIGN KEY (user_group_id) REFERENCES public.user_group(user_group_id) ON DELETE RESTRICT
);
CREATE INDEX user_active_idx ON public."user" USING btree (active);
CREATE INDEX user_expires_idx ON public."user" USING btree (expires);
CREATE INDEX user_hidden_idx ON public."user" USING btree (hidden);
CREATE INDEX user_mail_idx ON public."user" USING btree (mail);
CREATE INDEX user_name_idx ON public."user" USING btree (name);
CREATE INDEX user_ref_id_idx ON public."user" USING btree (ref_id);
CREATE INDEX user_user_group_id_idx ON public."user" USING btree (user_group_id);
CREATE INDEX user_username_idx ON public."user" USING btree (username);


-- public.user_event definition

-- Drop table

-- DROP TABLE public.user_event;

CREATE TABLE public.user_event (
	user_event_id serial4 NOT NULL,
	user_id int2 NULL,
	group_id int2 NULL,
	context_id int2 NULL,
	parent_id int4 NULL,
	"type" int2 DEFAULT 1 NOT NULL,
	title varchar(100) NOT NULL,
	description text NULL,
	date_start timestamptz NULL,
	date_end timestamptz NULL,
	all_day bool DEFAULT false NOT NULL,
	recurring jsonb NULL,
	uid text NULL,
	date_created timestamptz DEFAULT now() NOT NULL,
	date_modified timestamptz DEFAULT now() NOT NULL,
	conference text NULL,
	attendee jsonb NULL,
	"location" text NULL,
	CONSTRAINT user_event_pkey PRIMARY KEY (user_event_id),
	CONSTRAINT user_event_context_id_fkey FOREIGN KEY (context_id) REFERENCES public.event_context(context_id) ON DELETE CASCADE,
	CONSTRAINT user_event_group_id_fkey FOREIGN KEY (group_id) REFERENCES public.user_group(user_group_id) ON DELETE CASCADE,
	CONSTRAINT user_event_user_id_fkey FOREIGN KEY (user_id) REFERENCES public."user"(user_id) ON DELETE CASCADE
);
CREATE INDEX user_event_context_id ON public.user_event USING btree (context_id);
CREATE INDEX user_event_uid ON public.user_event USING btree (uid);
CREATE INDEX user_event_user_id ON public.user_event USING btree (user_id);

-- Table Triggers

create trigger user_event_lastmodified before
update
    on
    public.user_event for each row execute function user_event_lastmodified_column();


-- public.user_notification definition

-- Drop table

-- DROP TABLE public.user_notification;

CREATE TABLE public.user_notification (
	user_notification_id serial4 NOT NULL,
	user_id int2 NULL,
	context_id int2 NULL,
	notification_id int4 NULL,
	"read" bool DEFAULT false NOT NULL,
	CONSTRAINT user_notification_pkey PRIMARY KEY (user_notification_id),
	CONSTRAINT user_notification_context_id_fkey FOREIGN KEY (context_id) REFERENCES public.event_context(context_id) ON DELETE CASCADE,
	CONSTRAINT user_notification_notification_id_fkey FOREIGN KEY (notification_id) REFERENCES public.base_notification(notification_id) ON DELETE CASCADE,
	CONSTRAINT user_notification_user_id_fkey FOREIGN KEY (user_id) REFERENCES public."user"(user_id) ON DELETE CASCADE
);


-- public.user_session definition

-- Drop table

-- DROP TABLE public.user_session;

CREATE TABLE public.user_session (
	user_id int2 NOT NULL,
	"session" varchar(32) NOT NULL,
	date_start timestamptz NOT NULL,
	date_end timestamptz NULL,
	CONSTRAINT user_session_pkey PRIMARY KEY (user_id, session),
	CONSTRAINT user_session_user_id_fkey FOREIGN KEY (user_id) REFERENCES public."user"(user_id) ON DELETE CASCADE
);


-- public.warehouse definition

-- Drop table

-- DROP TABLE public.warehouse;

CREATE TABLE public.warehouse (
	warehouse_id serial4 NOT NULL,
	title varchar(100) DEFAULT ''::character varying NOT NULL,
	code varchar(100) DEFAULT ''::character varying NOT NULL,
	"comment" text DEFAULT ''::text NOT NULL,
	office_id int4 NULL,
	user_id int4 NULL,
	company_id int4 NULL,
	CONSTRAINT warehouse_pkey PRIMARY KEY (warehouse_id),
	CONSTRAINT warehouse_company_id_fkey FOREIGN KEY (company_id) REFERENCES public.company(company_id),
	CONSTRAINT warehouse_user_id_fkey FOREIGN KEY (user_id) REFERENCES public."user"(user_id) ON DELETE RESTRICT
);
CREATE INDEX warehouse_office_id ON public.warehouse USING btree (office_id);
CREATE INDEX warehouse_user_id ON public.warehouse USING btree (user_id);


-- public.warehouse_product_scrap definition

-- Drop table

-- DROP TABLE public.warehouse_product_scrap;

CREATE TABLE public.warehouse_product_scrap (
	scrap_id serial4 NOT NULL,
	user_id int4 NOT NULL,
	modified_id int4 NOT NULL,
	warehouse_id int4 NOT NULL,
	stamp timestamptz NULL,
	modified timestamptz NULL,
	"locked" bool DEFAULT false NOT NULL,
	locked_id int4 NULL,
	not_ids jsonb DEFAULT '[]'::jsonb NOT NULL,
	CONSTRAINT warehouse_product_scrap_pkey PRIMARY KEY (scrap_id),
	CONSTRAINT warehouse_product_scrap_warehouse_id_fkey FOREIGN KEY (warehouse_id) REFERENCES public.warehouse(warehouse_id) ON DELETE RESTRICT
);
CREATE INDEX warehouse_product_scrap_locked_id_idx ON public.warehouse_product_scrap USING btree (locked_id);
CREATE INDEX warehouse_product_scrap_locked_idx ON public.warehouse_product_scrap USING btree (locked);
CREATE INDEX warehouse_product_scrap_modified_id_idx ON public.warehouse_product_scrap USING btree (modified_id);
CREATE INDEX warehouse_product_scrap_modified_idx ON public.warehouse_product_scrap USING btree (modified);
CREATE INDEX warehouse_product_scrap_stamp_idx ON public.warehouse_product_scrap USING btree (stamp);
CREATE INDEX warehouse_product_scrap_user_id_idx ON public.warehouse_product_scrap USING btree (user_id);


-- public.warehouse_product_scrap_row definition

-- Drop table

-- DROP TABLE public.warehouse_product_scrap_row;

CREATE TABLE public.warehouse_product_scrap_row (
	row_id serial4 NOT NULL,
	scrap_id int4 NOT NULL,
	product_id int4 NOT NULL,
	quantity numeric(12, 2) NULL,
	reason_id int4 NOT NULL,
	CONSTRAINT warehouse_product_scrap_row_pkey PRIMARY KEY (row_id),
	CONSTRAINT warehouse_product_scrap_row_reason_id_fkey FOREIGN KEY (reason_id) REFERENCES public.warehouse_product_scrap_reason(reason_id) ON DELETE RESTRICT,
	CONSTRAINT warehouse_product_scrap_row_scrap_id_fkey FOREIGN KEY (scrap_id) REFERENCES public.warehouse_product_scrap(scrap_id) ON DELETE CASCADE
);
CREATE INDEX warehouse_product_scrap_row_product_id_idx ON public.warehouse_product_scrap_row USING btree (product_id);
CREATE INDEX warehouse_product_scrap_row_reason_id_idx ON public.warehouse_product_scrap_row USING btree (reason_id);
CREATE INDEX warehouse_product_scrap_row_scrap_id_idx ON public.warehouse_product_scrap_row USING btree (scrap_id);


-- public.warehouse_revision definition

-- Drop table

-- DROP TABLE public.warehouse_revision;

CREATE TABLE public.warehouse_revision (
	warehouse_revision_id serial4 NOT NULL,
	warehouse_id int4 NOT NULL,
	user_id int2 NOT NULL,
	"date" timestamptz DEFAULT now() NOT NULL,
	CONSTRAINT warehouse_revision_pkey PRIMARY KEY (warehouse_revision_id),
	CONSTRAINT warehouse_revision_warehouse_id_fkey FOREIGN KEY (warehouse_id) REFERENCES public.warehouse(warehouse_id) ON DELETE CASCADE
);
CREATE INDEX warehouse_revision_date_idx ON public.warehouse_revision USING btree (date);
CREATE INDEX warehouse_revision_user_id_idx ON public.warehouse_revision USING btree (user_id);
CREATE INDEX warehouse_revision_warehouse_id_idx ON public.warehouse_revision USING btree (warehouse_id);


-- public.warehouse_revision_data definition

-- Drop table

-- DROP TABLE public.warehouse_revision_data;

CREATE TABLE public.warehouse_revision_data (
	warehouse_revision_data_id serial4 NOT NULL,
	warehouse_revision_id int4 NOT NULL,
	product_id int4 NOT NULL,
	serial_id int4 NULL,
	quantity numeric(12, 2) DEFAULT 0 NOT NULL,
	quantity1 numeric(12, 2) DEFAULT 0 NOT NULL,
	price numeric(12, 4) DEFAULT 0 NOT NULL,
	price_currency_id int2 DEFAULT 0 NOT NULL,
	"comment" varchar(100) DEFAULT ''::character varying NULL,
	CONSTRAINT warehouse_revision_data_pkey PRIMARY KEY (warehouse_revision_data_id),
	CONSTRAINT warehouse_revision_data_warehouse_revision_id_fkey FOREIGN KEY (warehouse_revision_id) REFERENCES public.warehouse_revision(warehouse_revision_id) ON DELETE CASCADE
);
CREATE INDEX warehouse_revision_data_product_id ON public.warehouse_revision_data USING btree (product_id);
CREATE INDEX warehouse_revision_data_warehouse_revision_id ON public.warehouse_revision_data USING btree (warehouse_revision_id);


-- public.warehouse_transfer definition

-- Drop table

-- DROP TABLE public.warehouse_transfer;

CREATE TABLE public.warehouse_transfer (
	warehouse_transfer_id serial4 NOT NULL,
	warehouse_id_from int4 NOT NULL,
	warehouse_id_to int4 NOT NULL,
	user_id int2 NOT NULL,
	state int2 DEFAULT 0 NOT NULL,
	"comment" text NULL,
	"date" timestamptz DEFAULT now() NOT NULL,
	created_id int4 NOT NULL,
	modified_id int4 NOT NULL,
	modified timestamptz DEFAULT now() NOT NULL,
	do_notify jsonb DEFAULT '[]'::jsonb NOT NULL,
	lock_notify jsonb DEFAULT '[]'::jsonb NOT NULL,
	"locked" bool DEFAULT false NOT NULL,
	locked_id int4 NULL,
	ref_id int4 NULL,
	CONSTRAINT warehouse_transfer_pkey PRIMARY KEY (warehouse_transfer_id),
	CONSTRAINT warehouse_transfer_created_id_fkey FOREIGN KEY (created_id) REFERENCES public."user"(user_id) ON DELETE RESTRICT,
	CONSTRAINT warehouse_transfer_locked_id_fkey FOREIGN KEY (locked_id) REFERENCES public."user"(user_id) ON DELETE RESTRICT,
	CONSTRAINT warehouse_transfer_modified_id_fkey FOREIGN KEY (modified_id) REFERENCES public."user"(user_id) ON DELETE RESTRICT,
	CONSTRAINT warehouse_transfer_ref_id_fkey FOREIGN KEY (ref_id) REFERENCES public.warehouse_transfer(warehouse_transfer_id) ON DELETE RESTRICT
);
CREATE INDEX warehouse_transfer_created_id_idx ON public.warehouse_transfer USING btree (created_id);
CREATE INDEX warehouse_transfer_date_idx ON public.warehouse_transfer USING btree (date);
CREATE INDEX warehouse_transfer_do_notify_idx ON public.warehouse_transfer USING btree (do_notify);
CREATE INDEX warehouse_transfer_lock_notify_idx ON public.warehouse_transfer USING btree (lock_notify);
CREATE INDEX warehouse_transfer_locked_id_idx ON public.warehouse_transfer USING btree (locked_id);
CREATE INDEX warehouse_transfer_locked_idx ON public.warehouse_transfer USING btree (locked);
CREATE INDEX warehouse_transfer_modified_id_idx ON public.warehouse_transfer USING btree (modified_id);
CREATE INDEX warehouse_transfer_modified_idx ON public.warehouse_transfer USING btree (modified);
CREATE INDEX warehouse_transfer_ref_id_idx ON public.warehouse_transfer USING btree (ref_id);
CREATE INDEX warehouse_transfer_state_idx ON public.warehouse_transfer USING btree (state);
CREATE INDEX warehouse_transfer_user_id_idx ON public.warehouse_transfer USING btree (user_id);
CREATE INDEX warehouse_transfer_warehouse_id_from_idx ON public.warehouse_transfer USING btree (warehouse_id_from);
CREATE INDEX warehouse_transfer_warehouse_id_to_idx ON public.warehouse_transfer USING btree (warehouse_id_to);


-- public.warehouse_transfer_data definition

-- Drop table

-- DROP TABLE public.warehouse_transfer_data;

CREATE TABLE public.warehouse_transfer_data (
	warehouse_transfer_data_id serial4 NOT NULL,
	warehouse_transfer_id int4 NOT NULL,
	product_id int4 NOT NULL,
	quantity numeric(12, 2) DEFAULT 0 NOT NULL,
	serial_id int4 NULL,
	"position" int4 DEFAULT 0 NOT NULL,
	CONSTRAINT warehouse_transfer_data_pkey PRIMARY KEY (warehouse_transfer_data_id),
	CONSTRAINT warehouse_transfer_data_warehouse_transfer_id_fkey FOREIGN KEY (warehouse_transfer_id) REFERENCES public.warehouse_transfer(warehouse_transfer_id) ON DELETE CASCADE
);
CREATE INDEX warehouse_transfer_data_warehouse_transfer_id ON public.warehouse_transfer_data USING btree (warehouse_transfer_id);


-- public.wh_inventory_move definition

-- Drop table

-- DROP TABLE public.wh_inventory_move;

CREATE TABLE public.wh_inventory_move (
	move_id serial4 NOT NULL,
	product_id int4 NOT NULL,
	wh_from_id int4 NULL,
	wh_to_id int4 NULL,
	quantity numeric(12, 4) DEFAULT 0 NOT NULL,
	stamp timestamptz DEFAULT now() NOT NULL,
	reason varchar(120) NULL,
	CONSTRAINT wh_inventory_move_pkey PRIMARY KEY (move_id),
	CONSTRAINT wh_inventory_move_wh_from_id_fkey FOREIGN KEY (wh_from_id) REFERENCES public.warehouse(warehouse_id) ON DELETE RESTRICT,
	CONSTRAINT wh_inventory_move_wh_to_id_fkey FOREIGN KEY (wh_to_id) REFERENCES public.warehouse(warehouse_id) ON DELETE RESTRICT
);
CREATE INDEX wh_inventory_move_product_id_idx ON public.wh_inventory_move USING btree (product_id);
CREATE INDEX wh_inventory_move_quantity_idx ON public.wh_inventory_move USING btree (quantity);
CREATE INDEX wh_inventory_move_reason_idx ON public.wh_inventory_move USING btree (reason);
CREATE INDEX wh_inventory_move_stamp_idx ON public.wh_inventory_move USING btree (stamp);
CREATE INDEX wh_inventory_move_wh_from_id_idx ON public.wh_inventory_move USING btree (wh_from_id);
CREATE INDEX wh_inventory_move_wh_to_id_idx ON public.wh_inventory_move USING btree (wh_to_id);


-- public.xone_city definition

-- Drop table

-- DROP TABLE public.xone_city;

CREATE TABLE public.xone_city (
	city_id int8 NOT NULL,
	country_id int8 NOT NULL,
	"name" varchar(120) NOT NULL,
	pcity_id int8 NULL,
	"data" jsonb DEFAULT '{}'::jsonb NOT NULL,
	CONSTRAINT xone_city_pkey PRIMARY KEY (city_id),
	CONSTRAINT xone_city_country_id_fkey FOREIGN KEY (country_id) REFERENCES public.xone_country(country_id) ON DELETE CASCADE
);
CREATE INDEX xone_city_city_id_idx ON public.xone_city USING btree (city_id);
CREATE INDEX xone_city_country_id_idx ON public.xone_city USING btree (country_id);
CREATE INDEX xone_city_name_idx ON public.xone_city USING btree (name);
CREATE INDEX xone_city_pcity_id_idx ON public.xone_city USING btree (pcity_id);


-- public.xone_office definition

-- Drop table

-- DROP TABLE public.xone_office;

CREATE TABLE public.xone_office (
	office_id int8 NOT NULL,
	city_id int8 NOT NULL,
	"name" varchar(120) NOT NULL,
	lat numeric(12, 8) DEFAULT '-1'::integer NOT NULL,
	lon numeric(12, 8) DEFAULT '-1'::integer NOT NULL,
	"data" jsonb DEFAULT '{}'::jsonb NOT NULL,
	CONSTRAINT xone_office_pkey PRIMARY KEY (office_id),
	CONSTRAINT xone_office_city_id_fkey FOREIGN KEY (city_id) REFERENCES public.xone_city(city_id) ON DELETE CASCADE
);
CREATE INDEX xone_office_city_id_idx ON public.xone_office USING btree (city_id);
CREATE INDEX xone_office_lat_idx ON public.xone_office USING btree (lat);
CREATE INDEX xone_office_lon_idx ON public.xone_office USING btree (lon);
CREATE INDEX xone_office_name_idx ON public.xone_office USING btree (name);
CREATE INDEX xone_office_office_id_idx ON public.xone_office USING btree (office_id);


-- public.xone_street definition

-- Drop table

-- DROP TABLE public.xone_street;

CREATE TABLE public.xone_street (
	street_id int8 NOT NULL,
	city_id int8 NOT NULL,
	"name" varchar(120) NOT NULL,
	complex bool DEFAULT false NOT NULL,
	pstreet_id int8 NULL,
	"data" jsonb DEFAULT '{}'::jsonb NOT NULL,
	CONSTRAINT xone_street_pkey PRIMARY KEY (street_id),
	CONSTRAINT xone_street_city_id_fkey FOREIGN KEY (city_id) REFERENCES public.xone_city(city_id) ON DELETE CASCADE
);
CREATE INDEX xone_street_city_id_idx ON public.xone_street USING btree (city_id);
CREATE INDEX xone_street_complex_idx ON public.xone_street USING btree (complex);
CREATE INDEX xone_street_name_idx ON public.xone_street USING btree (name);
CREATE INDEX xone_street_pstreet_id_idx ON public.xone_street USING btree (pstreet_id);
CREATE INDEX xone_street_street_id_idx ON public.xone_street USING btree (street_id);


-- public.ac_device_track_backup definition

-- Drop table

-- DROP TABLE public.ac_device_track_backup;

CREATE TABLE public.ac_device_track_backup (
	backup_id serial4 NOT NULL,
	machine_id int4 NOT NULL,
	port int4 DEFAULT 22005 NOT NULL,
	profile varchar(80) NULL,
	project_name varchar(80) NULL,
	database_name varchar(80) NULL,
	"user" varchar(50) NULL,
	custom_lines text NULL,
	time_start int4 NULL,
	"interval" int4 NULL,
	active bool DEFAULT true NOT NULL,
	send_error bool DEFAULT false NOT NULL,
	address varchar(120) NULL,
	jaddress varchar(120) NULL,
	juser varchar(90) NULL,
	jport int4 DEFAULT 22005 NULL,
	CONSTRAINT ac_device_track_backup_pkey PRIMARY KEY (backup_id),
	CONSTRAINT ac_device_track_backup_machine_id_fkey FOREIGN KEY (machine_id) REFERENCES public.ac_device_track_machine(machine_id) ON DELETE CASCADE
);
CREATE INDEX ac_device_track_backup_active_idx ON public.ac_device_track_backup USING btree (active);
CREATE INDEX ac_device_track_backup_interval_idx ON public.ac_device_track_backup USING btree ("interval");
CREATE INDEX ac_device_track_backup_machine_id ON public.ac_device_track_backup USING btree (machine_id);
CREATE INDEX ac_device_track_backup_profile_idx ON public.ac_device_track_backup USING btree (profile);
CREATE INDEX ac_device_track_backup_project_name_idx ON public.ac_device_track_backup USING btree (project_name);
CREATE INDEX ac_device_track_backup_send_error_idx ON public.ac_device_track_backup USING btree (send_error);
CREATE INDEX ac_device_track_backup_time_start_idx ON public.ac_device_track_backup USING btree (time_start);


-- public.ac_device_track_backup_log definition

-- Drop table

-- DROP TABLE public.ac_device_track_backup_log;

CREATE TABLE public.ac_device_track_backup_log (
	log_id serial4 NOT NULL,
	backup_id int4 NOT NULL,
	stamp timestamptz NULL,
	stamp_end timestamptz NULL,
	exit_code int4 NULL,
	message text NULL,
	error text NULL,
	CONSTRAINT ac_device_track_backup_log_pkey PRIMARY KEY (log_id),
	CONSTRAINT ac_device_track_backup_log_backup_id_fkey FOREIGN KEY (backup_id) REFERENCES public.ac_device_track_backup(backup_id) ON DELETE CASCADE
);
CREATE INDEX ac_device_track_backup_log_backup_id ON public.ac_device_track_backup_log USING btree (backup_id);


-- public.ac_device_track_dev_ip definition

-- Drop table

-- DROP TABLE public.ac_device_track_dev_ip;

CREATE TABLE public.ac_device_track_dev_ip (
	ip_id serial4 NOT NULL,
	machine_id int4 NOT NULL,
	ip varchar(40) NULL,
	stamp timestamptz DEFAULT now() NOT NULL,
	CONSTRAINT ac_device_track_dev_ip_pkey PRIMARY KEY (ip_id),
	CONSTRAINT ac_device_track_dev_ip_machine_id_fkey FOREIGN KEY (machine_id) REFERENCES public.ac_device_track_machine(machine_id) ON DELETE CASCADE
);
CREATE INDEX ac_device_track_dev_ip_machine_id_idx ON public.ac_device_track_dev_ip USING btree (machine_id);
CREATE INDEX ac_device_track_dev_ip_stamp_idx ON public.ac_device_track_dev_ip USING btree (stamp);


-- public.ac_device_track_diag_data definition

-- Drop table

-- DROP TABLE public.ac_device_track_diag_data;

CREATE TABLE public.ac_device_track_diag_data (
	dd_id bigserial NOT NULL,
	machine_id int4 NOT NULL,
	stamp timestamptz DEFAULT now() NOT NULL,
	"data" jsonb DEFAULT '{}'::jsonb NOT NULL,
	CONSTRAINT ac_device_track_diag_data_pkey PRIMARY KEY (dd_id),
	CONSTRAINT ac_device_track_diag_data_machine_id_fkey FOREIGN KEY (machine_id) REFERENCES public.ac_device_track_machine(machine_id) ON DELETE CASCADE
);
CREATE INDEX ac_device_track_diag_data_machine_id_idx ON public.ac_device_track_diag_data USING btree (machine_id);
CREATE INDEX ac_device_track_diag_data_stamp_idx ON public.ac_device_track_diag_data USING btree (stamp);


-- public.ac_device_track_dtickets definition

-- Drop table

-- DROP TABLE public.ac_device_track_dtickets;

CREATE TABLE public.ac_device_track_dtickets (
	dt_id bigserial NOT NULL,
	machine_id int4 NOT NULL,
	stamp timestamptz DEFAULT now() NOT NULL,
	tickets int2 DEFAULT 5 NOT NULL,
	CONSTRAINT ac_device_track_dtickets_pkey PRIMARY KEY (dt_id),
	CONSTRAINT ac_device_track_dtickets_machine_id_fkey FOREIGN KEY (machine_id) REFERENCES public.ac_device_track_machine(machine_id) ON DELETE CASCADE
);
CREATE INDEX ac_device_track_dtickets_machine_id_idx ON public.ac_device_track_dtickets USING btree (machine_id);
CREATE INDEX ac_device_track_dtickets_stamp_idx ON public.ac_device_track_dtickets USING btree (stamp);
CREATE INDEX ac_device_track_dtickets_tickets_idx ON public.ac_device_track_dtickets USING btree (tickets);


-- public.ac_device_track_license definition

-- Drop table

-- DROP TABLE public.ac_device_track_license;

CREATE TABLE public.ac_device_track_license (
	license_id serial4 NOT NULL,
	machine_id int4 NOT NULL,
	ts_start timestamptz NULL,
	ts_end timestamptz NULL,
	ts_renew timestamptz NULL,
	allowed_devices int4 DEFAULT 2 NOT NULL,
	enabled bool NULL,
	modules jsonb DEFAULT '[]'::jsonb NOT NULL,
	responsible jsonb DEFAULT '[]'::jsonb NOT NULL,
	ev_ids jsonb DEFAULT '[]'::jsonb NOT NULL,
	server_port int4 NULL,
	allowed_users int2 DEFAULT 0 NOT NULL,
	allowed_online int2 DEFAULT 0 NOT NULL,
	username text NULL,
	passwd text NULL,
	offline_time int4 DEFAULT 0 NOT NULL,
	gather_diags bool DEFAULT false NOT NULL,
	pinv_id int4 NULL,
	CONSTRAINT ac_device_track_license_pkey PRIMARY KEY (license_id),
	CONSTRAINT ac_device_track_license_machine_id_fkey FOREIGN KEY (machine_id) REFERENCES public.ac_device_track_machine(machine_id) ON DELETE CASCADE
);
CREATE INDEX ac_device_track_license_allowed_online_idx ON public.ac_device_track_license USING btree (allowed_online);
CREATE INDEX ac_device_track_license_allowed_users_idx ON public.ac_device_track_license USING btree (allowed_users);
CREATE INDEX ac_device_track_license_enabled_idx ON public.ac_device_track_license USING btree (enabled);
CREATE INDEX ac_device_track_license_gather_diags_idx ON public.ac_device_track_license USING btree (gather_diags);
CREATE INDEX ac_device_track_license_machine_id_idx ON public.ac_device_track_license USING btree (machine_id);
CREATE INDEX ac_device_track_license_offline_time_idx ON public.ac_device_track_license USING btree (offline_time);
CREATE INDEX ac_device_track_license_pinv_id_idx ON public.ac_device_track_license USING btree (pinv_id);
CREATE INDEX ac_device_track_license_server_port_idx ON public.ac_device_track_license USING btree (server_port);
CREATE INDEX ac_device_track_license_ts_end_idx ON public.ac_device_track_license USING btree (ts_end);
CREATE INDEX ac_device_track_license_ts_renew_idx ON public.ac_device_track_license USING btree (ts_renew);
CREATE INDEX ac_device_track_license_ts_start_idx ON public.ac_device_track_license USING btree (ts_start);


-- public.auth_import_calendar definition

-- Drop table

-- DROP TABLE public.auth_import_calendar;

CREATE TABLE public.auth_import_calendar (
	cal_id bigserial NOT NULL,
	user_id int4 NOT NULL,
	"name" varchar(120) NULL,
	"path" text NOT NULL,
	color int2 DEFAULT 5 NOT NULL,
	last_import timestamptz DEFAULT '1900-01-01 02:00:00+02'::timestamp with time zone NOT NULL,
	CONSTRAINT auth_import_calendar_pkey PRIMARY KEY (cal_id),
	CONSTRAINT auth_import_calendar_user_id_fkey FOREIGN KEY (user_id) REFERENCES public."user"(user_id) ON DELETE CASCADE
);
CREATE INDEX auth_import_calendar_last_import_idx ON public.auth_import_calendar USING btree (last_import);
CREATE INDEX auth_import_calendar_name_idx ON public.auth_import_calendar USING btree (name);
CREATE INDEX auth_import_calendar_user_id_idx ON public.auth_import_calendar USING btree (user_id);


-- public.auth_mail_account definition

-- Drop table

-- DROP TABLE public.auth_mail_account;

CREATE TABLE public.auth_mail_account (
	account_id serial4 NOT NULL,
	user_id int4 NOT NULL,
	mail varchar(40) NULL,
	settings jsonb NULL,
	CONSTRAINT auth_mail_account_pkey PRIMARY KEY (account_id),
	CONSTRAINT auth_mail_account_user_id_fkey FOREIGN KEY (user_id) REFERENCES public."user"(user_id) ON DELETE RESTRICT
);
CREATE INDEX auth_mail_account_mail_idx ON public.auth_mail_account USING btree (mail);
CREATE INDEX auth_mail_account_user_id_idx ON public.auth_mail_account USING btree (user_id);


-- public.auth_mail_message definition

-- Drop table

-- DROP TABLE public.auth_mail_message;

CREATE TABLE public.auth_mail_message (
	msg_id serial4 NOT NULL,
	stamp timestamptz DEFAULT now() NOT NULL,
	user_id int4 NOT NULL,
	status int2 DEFAULT 0 NOT NULL,
	type_id int4 NULL,
	title text NULL,
	"from" varchar(120) NULL,
	uid varchar(120) NULL,
	to_mail_id int4 NOT NULL,
	body text NULL,
	CONSTRAINT auth_mail_message_pkey PRIMARY KEY (msg_id),
	CONSTRAINT auth_mail_message_to_mail_id_fkey FOREIGN KEY (to_mail_id) REFERENCES public.auth_mail_account(account_id) ON DELETE RESTRICT,
	CONSTRAINT auth_mail_message_type_id_fkey FOREIGN KEY (type_id) REFERENCES public.auth_mail_type(type_id) ON DELETE RESTRICT,
	CONSTRAINT auth_mail_message_user_id_fkey FOREIGN KEY (user_id) REFERENCES public."user"(user_id) ON DELETE RESTRICT
);
CREATE INDEX auth_mail_message_from_idx ON public.auth_mail_message USING btree ("from");
CREATE INDEX auth_mail_message_stamp_idx ON public.auth_mail_message USING btree (stamp);
CREATE INDEX auth_mail_message_status_idx ON public.auth_mail_message USING btree (status);
CREATE INDEX auth_mail_message_title_idx ON public.auth_mail_message USING btree (title);
CREATE INDEX auth_mail_message_to_mail_id_idx ON public.auth_mail_message USING btree (to_mail_id);
CREATE INDEX auth_mail_message_type_id_idx ON public.auth_mail_message USING btree (type_id);
CREATE INDEX auth_mail_message_uid_idx ON public.auth_mail_message USING btree (uid);
CREATE INDEX auth_mail_message_user_id_idx ON public.auth_mail_message USING btree (user_id);


-- public.auth_phone_log definition

-- Drop table

-- DROP TABLE public.auth_phone_log;

CREATE TABLE public.auth_phone_log (
	log_id bigserial NOT NULL,
	user_id int4 NOT NULL,
	stamp timestamptz DEFAULT now() NOT NULL,
	log text NULL,
	CONSTRAINT auth_phone_log_pkey PRIMARY KEY (log_id),
	CONSTRAINT auth_phone_log_user_id_fkey FOREIGN KEY (user_id) REFERENCES public."user"(user_id) ON DELETE CASCADE
);
CREATE INDEX auth_phone_log_stamp_idx ON public.auth_phone_log USING btree (stamp);
CREATE INDEX auth_phone_log_user_id_idx ON public.auth_phone_log USING btree (user_id);


-- public.auth_push_keys definition

-- Drop table

-- DROP TABLE public.auth_push_keys;

CREATE TABLE public.auth_push_keys (
	key_id serial4 NOT NULL,
	params jsonb NULL,
	user_id int4 NOT NULL,
	imported timestamptz DEFAULT now() NOT NULL,
	CONSTRAINT auth_push_keys_pkey PRIMARY KEY (key_id),
	CONSTRAINT auth_push_keys_user_id_fkey FOREIGN KEY (user_id) REFERENCES public."user"(user_id) ON DELETE CASCADE
);
CREATE INDEX auth_push_keys_imported_idx ON public.auth_push_keys USING btree (imported);
CREATE INDEX auth_push_keys_user_id_idx ON public.auth_push_keys USING btree (user_id);


-- public.auth_session_log definition

-- Drop table

-- DROP TABLE public.auth_session_log;

CREATE TABLE public.auth_session_log (
	log_id serial4 NOT NULL,
	user_id int4 NOT NULL,
	"session" varchar(60) NULL,
	"action" int4 NOT NULL,
	stamp timestamptz NULL,
	ip varchar(20) NULL,
	seconds int4 NULL,
	CONSTRAINT auth_session_log_pkey PRIMARY KEY (log_id),
	CONSTRAINT auth_session_log_user_id_fkey FOREIGN KEY (user_id) REFERENCES public."user"(user_id) ON DELETE RESTRICT
);
CREATE INDEX auth_session_log_action ON public.auth_session_log USING btree (action);
CREATE INDEX auth_session_log_ip ON public.auth_session_log USING btree (ip);
CREATE INDEX auth_session_log_session ON public.auth_session_log USING btree (session);
CREATE INDEX auth_session_log_stamp ON public.auth_session_log USING btree (stamp);
CREATE INDEX auth_session_log_user_id ON public.auth_session_log USING btree (user_id);


-- public.auth_sms_msg definition

-- Drop table

-- DROP TABLE public.auth_sms_msg;

CREATE TABLE public.auth_sms_msg (
	msg_id bigserial NOT NULL,
	sent timestamptz DEFAULT now() NOT NULL,
	phone varchar(150) NULL,
	message text NULL,
	error text NULL,
	user_id int4 NOT NULL,
	CONSTRAINT auth_sms_msg_pkey PRIMARY KEY (msg_id),
	CONSTRAINT auth_sms_msg_user_id_fkey FOREIGN KEY (user_id) REFERENCES public."user"(user_id) ON DELETE SET NULL
);
CREATE INDEX auth_sms_msg_phone_idx ON public.auth_sms_msg USING btree (phone);
CREATE INDEX auth_sms_msg_sent_idx ON public.auth_sms_msg USING btree (sent);
CREATE INDEX auth_sms_msg_user_id_idx ON public.auth_sms_msg USING btree (user_id);


-- public.auth_temp_password definition

-- Drop table

-- DROP TABLE public.auth_temp_password;

CREATE TABLE public.auth_temp_password (
	user_id int4 NOT NULL,
	"password" varchar(90) NULL,
	expires timestamptz NOT NULL,
	CONSTRAINT auth_temp_password_pkey PRIMARY KEY (user_id),
	CONSTRAINT auth_temp_password_user_id_fkey FOREIGN KEY (user_id) REFERENCES public."user"(user_id) ON DELETE CASCADE
);
CREATE INDEX auth_temp_password_expires_idx ON public.auth_temp_password USING btree (expires);


-- public.auth_u_order definition

-- Drop table

-- DROP TABLE public.auth_u_order;

CREATE TABLE public.auth_u_order (
	ord_id serial4 NOT NULL,
	parent_id int4 NULL,
	user_id int4 NULL,
	lft int4 DEFAULT 0 NOT NULL,
	rgt int4 DEFAULT 0 NOT NULL,
	CONSTRAINT auth_u_order_pkey PRIMARY KEY (ord_id),
	CONSTRAINT auth_u_order_user_id_fkey FOREIGN KEY (user_id) REFERENCES public."user"(user_id) ON DELETE RESTRICT
);
CREATE INDEX auth_u_order_lft_idx ON public.auth_u_order USING btree (lft);
CREATE INDEX auth_u_order_parent_id_idx ON public.auth_u_order USING btree (parent_id);
CREATE INDEX auth_u_order_rgt_idx ON public.auth_u_order USING btree (rgt);
CREATE INDEX auth_u_order_user_id_idx ON public.auth_u_order USING btree (user_id);

-- Table Triggers

create trigger auth_u_order_delete before
delete
    on
    public.auth_u_order for each row
    when ((pg_trigger_depth() = 0)) execute function auth_u_order_delete();
create trigger auth_u_order_insert after
insert
    on
    public.auth_u_order for each row
    when ((new.parent_id <> 0)) execute function auth_u_order_insert();
create trigger auth_u_order_update after
update
    on
    public.auth_u_order for each row
    when ((old.parent_id is distinct
from
    new.parent_id)) execute function auth_u_order_update();


-- public.auth_work_day definition

-- Drop table

-- DROP TABLE public.auth_work_day;

CREATE TABLE public.auth_work_day (
	wd_id serial4 NOT NULL,
	user_id int4 NOT NULL,
	stamp timestamptz DEFAULT now() NOT NULL,
	seconds int8 DEFAULT 0 NOT NULL,
	"last" timestamptz DEFAULT now() NOT NULL,
	CONSTRAINT auth_work_day_pkey PRIMARY KEY (wd_id),
	CONSTRAINT auth_work_day_user_id_fkey FOREIGN KEY (user_id) REFERENCES public."user"(user_id) ON DELETE CASCADE
);
CREATE INDEX auth_work_day_last_idx ON public.auth_work_day USING btree (last);
CREATE INDEX auth_work_day_seconds_idx ON public.auth_work_day USING btree (seconds);
CREATE INDEX auth_work_day_stamp_idx ON public.auth_work_day USING btree (stamp);
CREATE INDEX auth_work_day_user_id_idx ON public.auth_work_day USING btree (user_id);


-- public.case_bnb_export definition

-- Drop table

-- DROP TABLE public.case_bnb_export;

CREATE TABLE public.case_bnb_export (
	exp_id bigserial NOT NULL,
	stamp timestamptz DEFAULT now() NOT NULL,
	imp_stamp timestamptz NULL,
	exported_id int4 NOT NULL,
	imported_id int4 NULL,
	person_id jsonb DEFAULT '[]'::jsonb NOT NULL,
	person_id_found jsonb DEFAULT '[]'::jsonb NOT NULL,
	CONSTRAINT case_bnb_export_pkey PRIMARY KEY (exp_id),
	CONSTRAINT case_bnb_export_exported_id FOREIGN KEY (exported_id) REFERENCES public."user"(user_id) ON DELETE RESTRICT,
	CONSTRAINT case_bnb_export_imported_id FOREIGN KEY (imported_id) REFERENCES public."user"(user_id) ON DELETE RESTRICT
);
CREATE INDEX case_bnb_export_exported_id_idx ON public.case_bnb_export USING btree (exported_id);
CREATE INDEX case_bnb_export_imp_stamp_idx ON public.case_bnb_export USING btree (imp_stamp);
CREATE INDEX case_bnb_export_imported_id_idx ON public.case_bnb_export USING btree (imported_id);
CREATE INDEX case_bnb_export_person_id_found_idx ON public.case_bnb_export USING btree (person_id_found);
CREATE INDEX case_bnb_export_person_id_idx ON public.case_bnb_export USING btree (person_id);
CREATE INDEX case_bnb_export_stamp_idx ON public.case_bnb_export USING btree (stamp);


-- public.case_case definition

-- Drop table

-- DROP TABLE public.case_case;

CREATE TABLE public.case_case (
	case_id serial4 NOT NULL,
	stamp timestamptz DEFAULT now() NOT NULL,
	case_number varchar(60) NOT NULL,
	debtor_id int4 NULL,
	responsible jsonb DEFAULT '[]'::jsonb NOT NULL,
	case_type int2 DEFAULT 0 NOT NULL,
	case_status int2 DEFAULT 0 NOT NULL,
	mem18 bool DEFAULT false NOT NULL,
	given_pdi bool DEFAULT false NOT NULL,
	nap bool DEFAULT false NOT NULL,
	other_cases bool DEFAULT false NOT NULL,
	other_chsi bool DEFAULT false NOT NULL,
	in_solidarity bool DEFAULT false NOT NULL,
	"mode" int2 DEFAULT 2 NOT NULL,
	calc_params jsonb DEFAULT '{}'::jsonb NOT NULL,
	t26_sep bool DEFAULT false NOT NULL,
	risk int2 DEFAULT 0 NOT NULL,
	location_id int4 NULL,
	chsi_bank_account varchar(120) NULL,
	distribute bool DEFAULT false NOT NULL,
	distr_sch bool DEFAULT false NOT NULL,
	arrived_money numeric(12, 3) DEFAULT 0 NOT NULL,
	abort_ret bool DEFAULT false NOT NULL,
	finish_ret bool DEFAULT false NOT NULL,
	last_doc timestamptz DEFAULT '1900-01-01 00:00:00+02'::timestamp with time zone NOT NULL,
	m_status int2 DEFAULT 0 NOT NULL,
	checked_pdi bool DEFAULT false NOT NULL,
	csn_id int8 NULL,
	non_worked timestamptz NULL,
	peremption timestamptz NULL,
	arrived_money_cred numeric(12, 3) DEFAULT 0 NOT NULL,
	first_doc_stamp timestamptz NULL,
	ret_amount numeric(12, 3) DEFAULT 0 NOT NULL,
	debtor_lst jsonb DEFAULT '[]'::jsonb NOT NULL,
	creditor_lst jsonb DEFAULT '[]'::jsonb NOT NULL,
	last_payment timestamptz DEFAULT '1900-01-01 00:00:00+02'::timestamp with time zone NOT NULL,
	act_debtor_lst jsonb DEFAULT '[]'::jsonb NOT NULL,
	act_creditor_lst jsonb DEFAULT '[]'::jsonb NOT NULL,
	presenter_lst jsonb DEFAULT '[]'::jsonb NOT NULL,
	lcheck timestamptz DEFAULT '1900-01-01 00:00:00+02'::timestamp with time zone NOT NULL,
	lcheck_bnb timestamptz DEFAULT '1900-01-01 00:00:00+02'::timestamp with time zone NOT NULL,
	lcheck_wc timestamptz DEFAULT '1900-01-01 00:00:00+02'::timestamp with time zone NOT NULL,
	lcheck_pns timestamptz DEFAULT '1900-01-01 00:00:00+02'::timestamp with time zone NOT NULL,
	lcheck_rest timestamptz DEFAULT '1900-01-01 00:00:00+02'::timestamp with time zone NOT NULL,
	last_in_doc timestamptz DEFAULT '1900-01-01 00:00:00+02'::timestamp with time zone NOT NULL,
	last_doc_sstype_id int4 NULL,
	last_doc_addon_flags int4 NULL,
	last_deed timestamptz DEFAULT '1900-01-01 00:00:00+02'::timestamp with time zone NOT NULL,
	debtor_phone bool DEFAULT false NOT NULL,
	arch_id int4 NULL,
	arch_year int4 DEFAULT 1900 NOT NULL,
	arch_cont int4 DEFAULT 0 NOT NULL,
	CONSTRAINT case_case_case_number_key UNIQUE (case_number),
	CONSTRAINT case_case_pkey PRIMARY KEY (case_id),
	CONSTRAINT case_case_arch_id_fkey FOREIGN KEY (arch_id) REFERENCES public.case_archive(arch_id) ON DELETE SET NULL,
	CONSTRAINT case_case_debtor_id FOREIGN KEY (debtor_id) REFERENCES public.customer(customer_id) ON DELETE RESTRICT,
	CONSTRAINT case_case_location_id FOREIGN KEY (location_id) REFERENCES public."user"(user_id) ON DELETE RESTRICT
);
CREATE INDEX case_case_abort_ret_idx ON public.case_case USING btree (abort_ret);
CREATE INDEX case_case_act_creditor_lst_idx ON public.case_case USING btree (act_creditor_lst);
CREATE INDEX case_case_act_creditor_lst_idx1 ON public.case_case USING btree (act_creditor_lst);
CREATE INDEX case_case_act_debtor_lst_idx ON public.case_case USING btree (act_debtor_lst);
CREATE INDEX case_case_arch_cont_idx ON public.case_case USING btree (arch_cont);
CREATE INDEX case_case_arch_id_idx ON public.case_case USING btree (arch_id);
CREATE INDEX case_case_arch_year_idx ON public.case_case USING btree (arch_year);
CREATE INDEX case_case_arrived_money_idx ON public.case_case USING btree (arrived_money);
CREATE INDEX case_case_arrived_money_idx1 ON public.case_case USING btree (arrived_money);
CREATE INDEX case_case_case_number_idx ON public.case_case USING btree (case_number);
CREATE INDEX case_case_case_status_idx ON public.case_case USING btree (case_status);
CREATE INDEX case_case_case_type_idx ON public.case_case USING btree (case_type);
CREATE INDEX case_case_checked_pdi_idx ON public.case_case USING btree (checked_pdi);
CREATE INDEX case_case_creditor_lst_idx ON public.case_case USING btree (creditor_lst);
CREATE INDEX case_case_csn_id_idx ON public.case_case USING btree (csn_id);
CREATE INDEX case_case_debtor_id_idx ON public.case_case USING btree (debtor_id);
CREATE INDEX case_case_debtor_lst_idx ON public.case_case USING btree (debtor_lst);
CREATE INDEX case_case_debtor_phone_idx ON public.case_case USING btree (debtor_phone);
CREATE INDEX case_case_distr_sch_idx ON public.case_case USING btree (distr_sch);
CREATE INDEX case_case_distribute_idx ON public.case_case USING btree (distribute);
CREATE INDEX case_case_finish_ret_idx ON public.case_case USING btree (finish_ret);
CREATE INDEX case_case_first_doc_stamp_idx ON public.case_case USING btree (first_doc_stamp);
CREATE INDEX case_case_given_pdi_idx ON public.case_case USING btree (given_pdi);
CREATE INDEX case_case_in_solidarity_idx ON public.case_case USING btree (in_solidarity);
CREATE INDEX case_case_last_deed_idx ON public.case_case USING btree (last_deed);
CREATE INDEX case_case_last_doc_addon_flags_idx ON public.case_case USING btree (last_doc_addon_flags);
CREATE INDEX case_case_last_doc_idx ON public.case_case USING btree (last_doc);
CREATE INDEX case_case_last_doc_sstype_id_idx ON public.case_case USING btree (last_doc_sstype_id);
CREATE INDEX case_case_last_in_doc_idx ON public.case_case USING btree (last_in_doc);
CREATE INDEX case_case_last_payment_idx ON public.case_case USING btree (last_payment);
CREATE INDEX case_case_lcheck_bnb_idx ON public.case_case USING btree (lcheck_bnb);
CREATE INDEX case_case_lcheck_idx ON public.case_case USING btree (lcheck);
CREATE INDEX case_case_lcheck_pns_idx ON public.case_case USING btree (lcheck_pns);
CREATE INDEX case_case_lcheck_rest_idx ON public.case_case USING btree (lcheck_rest);
CREATE INDEX case_case_lcheck_wc_idx ON public.case_case USING btree (lcheck_wc);
CREATE INDEX case_case_location_id_idx ON public.case_case USING btree (location_id);
CREATE INDEX case_case_m_status_idx ON public.case_case USING btree (m_status);
CREATE INDEX case_case_mem18_idx ON public.case_case USING btree (mem18);
CREATE INDEX case_case_mode_idx ON public.case_case USING btree (mode);
CREATE INDEX case_case_nap_idx ON public.case_case USING btree (nap);
CREATE INDEX case_case_non_worked_idx ON public.case_case USING btree (non_worked);
CREATE INDEX case_case_other_cases_idx ON public.case_case USING btree (other_cases);
CREATE INDEX case_case_other_chsi_idx ON public.case_case USING btree (other_chsi);
CREATE INDEX case_case_peremption_idx ON public.case_case USING btree (peremption);
CREATE INDEX case_case_responsible_idx ON public.case_case USING btree (responsible);
CREATE INDEX case_case_ret_amount_idx ON public.case_case USING btree (ret_amount);
CREATE INDEX case_case_risk_idx ON public.case_case USING btree (risk);
CREATE INDEX case_case_stamp_idx ON public.case_case USING btree (stamp);
CREATE INDEX case_case_t26_sep_idx ON public.case_case USING btree (t26_sep);


-- public.case_case_deed definition

-- Drop table

-- DROP TABLE public.case_case_deed;

CREATE TABLE public.case_case_deed (
	deed_id serial4 NOT NULL,
	case_id int4 NOT NULL,
	stamp timestamptz DEFAULT now() NOT NULL,
	ref_type int2 DEFAULT 1::smallint NOT NULL,
	ref_id int4 NULL,
	person_id int4 NULL,
	used bool DEFAULT false NOT NULL,
	day_number int4 DEFAULT 0 NOT NULL,
	printed bool DEFAULT false NOT NULL,
	replied bool DEFAULT false NOT NULL,
	CONSTRAINT case_case_deed_pkey PRIMARY KEY (deed_id),
	CONSTRAINT case_case_deed_case_id_fkey FOREIGN KEY (case_id) REFERENCES public.case_case(case_id) ON DELETE CASCADE,
	CONSTRAINT case_case_deed_person_id_fkey FOREIGN KEY (person_id) REFERENCES public.case_person(person_id) ON DELETE CASCADE
);
CREATE INDEX case_case_deed_case_id_idx ON public.case_case_deed USING btree (case_id);
CREATE INDEX case_case_deed_person_id_idx ON public.case_case_deed USING btree (person_id);
CREATE INDEX case_case_deed_printed_idx ON public.case_case_deed USING btree (printed);
CREATE INDEX case_case_deed_ref_id_idx ON public.case_case_deed USING btree (ref_id);
CREATE INDEX case_case_deed_ref_type_idx ON public.case_case_deed USING btree (ref_type);
CREATE INDEX case_case_deed_replied_idx ON public.case_case_deed USING btree (replied);
CREATE INDEX case_case_deed_stamp_idx ON public.case_case_deed USING btree (stamp);
CREATE INDEX case_case_deed_used_idx ON public.case_case_deed USING btree (used);


-- public.case_case_status definition

-- Drop table

-- DROP TABLE public.case_case_status;

CREATE TABLE public.case_case_status (
	st_id serial4 NOT NULL,
	case_id int4 NOT NULL,
	stamp timestamptz DEFAULT now() NOT NULL,
	status int2 DEFAULT 0::smallint NOT NULL,
	"comment" text NULL,
	sstatus int2 NULL,
	user_id int4 NULL,
	changed timestamptz DEFAULT now() NOT NULL,
	CONSTRAINT case_case_status_pkey PRIMARY KEY (st_id),
	CONSTRAINT case_case_status_case_id_fkey FOREIGN KEY (case_id) REFERENCES public.case_case(case_id) ON DELETE CASCADE
);
CREATE INDEX case_case_status_case_id_idx ON public.case_case_status USING btree (case_id);
CREATE INDEX case_case_status_changed_idx ON public.case_case_status USING btree (changed);
CREATE INDEX case_case_status_sstatus_idx ON public.case_case_status USING btree (sstatus);
CREATE INDEX case_case_status_stamp_idx ON public.case_case_status USING btree (stamp);
CREATE INDEX case_case_status_status_idx ON public.case_case_status USING btree (status);
CREATE INDEX case_case_status_user_id_idx ON public.case_case_status USING btree (user_id);


-- public.case_cred_account definition

-- Drop table

-- DROP TABLE public.case_cred_account;

CREATE TABLE public.case_cred_account (
	case_id int8 NOT NULL,
	cred_ids jsonb DEFAULT '[]'::jsonb NOT NULL,
	accounts jsonb DEFAULT '[]'::jsonb NOT NULL,
	CONSTRAINT case_cred_account_pkey PRIMARY KEY (case_id),
	CONSTRAINT case_cred_account_case_id_fkey FOREIGN KEY (case_id) REFERENCES public.case_case(case_id) ON DELETE CASCADE
);
CREATE INDEX case_cred_account_case_id_idx ON public.case_cred_account USING btree (case_id);


-- public.case_cust_data definition

-- Drop table

-- DROP TABLE public.case_cust_data;

CREATE TABLE public.case_cust_data (
	cd_id serial4 NOT NULL,
	case_id int4 NOT NULL,
	created timestamptz DEFAULT now() NOT NULL,
	modified timestamptz DEFAULT now() NOT NULL,
	created_id int4 NOT NULL,
	modified_id int4 NOT NULL,
	"role" int2 DEFAULT 0 NOT NULL,
	active bool DEFAULT true NOT NULL,
	person_id int4 NULL,
	ref_id int8 NULL,
	CONSTRAINT case_cust_data_pkey PRIMARY KEY (cd_id),
	CONSTRAINT case_cust_data_case_id_fkey FOREIGN KEY (case_id) REFERENCES public.case_case(case_id) ON DELETE CASCADE,
	CONSTRAINT case_cust_data_person_id_fkey FOREIGN KEY (person_id) REFERENCES public.case_person(person_id) ON DELETE RESTRICT,
	CONSTRAINT case_cust_data_ref_id_fkey FOREIGN KEY (ref_id) REFERENCES public.case_person(person_id) ON DELETE RESTRICT
);
CREATE INDEX case_cust_data_active_idx ON public.case_cust_data USING btree (active);
CREATE INDEX case_cust_data_case_id_idx ON public.case_cust_data USING btree (case_id);
CREATE INDEX case_cust_data_created_id_idx ON public.case_cust_data USING btree (created_id);
CREATE INDEX case_cust_data_created_idx ON public.case_cust_data USING btree (created);
CREATE INDEX case_cust_data_modified_id_idx ON public.case_cust_data USING btree (modified_id);
CREATE INDEX case_cust_data_modified_idx ON public.case_cust_data USING btree (modified);
CREATE INDEX case_cust_data_person_id_idx ON public.case_cust_data USING btree (person_id);
CREATE INDEX case_cust_data_ref_id_idx ON public.case_cust_data USING btree (ref_id);
CREATE INDEX case_cust_data_role_idx ON public.case_cust_data USING btree (role);


-- public.case_dist_track definition

-- Drop table

-- DROP TABLE public.case_dist_track;

CREATE TABLE public.case_dist_track (
	row_id bigserial NOT NULL,
	person_id int8 NOT NULL,
	case_id int8 NOT NULL,
	active bool DEFAULT true NOT NULL,
	type_id int2 DEFAULT 0 NOT NULL,
	ref_id int8 NULL,
	proc_ids jsonb DEFAULT '[]'::jsonb NOT NULL,
	created timestamptz DEFAULT now() NOT NULL,
	modified timestamptz DEFAULT now() NOT NULL,
	ld_created timestamptz NULL,
	ld_confirmed timestamptz NULL,
	ld_raised timestamptz NULL,
	ld_raise_confirmed timestamptz NULL,
	act text NULL,
	amount numeric(12, 3) DEFAULT 0 NOT NULL,
	currency_id int2 DEFAULT 1 NOT NULL,
	ba_exists bool DEFAULT false NOT NULL,
	ba_active bool DEFAULT false NOT NULL,
	ba_distrainted bool DEFAULT false NOT NULL,
	ld_printed timestamptz NULL,
	ld_raise_printed timestamptz NULL,
	ld_replied timestamptz NULL,
	ld_raise_replied timestamptz NULL,
	ld_printed_inst timestamptz NULL,
	ld_confirmed_inst timestamptz NULL,
	ld_replied_inst timestamptz NULL,
	ld_printed_reg timestamptz NULL,
	ld_confirmed_reg timestamptz NULL,
	ld_replied_reg timestamptz NULL,
	ld_raise_printed_inst timestamptz NULL,
	ld_raise_confirmed_inst timestamptz NULL,
	ld_raise_replied_inst timestamptz NULL,
	ld_raise_printed_reg timestamptz NULL,
	ld_raise_confirmed_reg timestamptz NULL,
	ld_raise_replied_reg timestamptz NULL,
	ld_types int4 NULL,
	ld_types_inst int4 NULL,
	ld_types_reg int4 NULL,
	ldr_types int4 NULL,
	ldr_types_inst int4 NULL,
	ldr_types_reg int4 NULL,
	CONSTRAINT case_dist_track_pkey PRIMARY KEY (row_id),
	CONSTRAINT case_dist_track_case_id_fkey FOREIGN KEY (case_id) REFERENCES public.case_case(case_id) ON DELETE CASCADE,
	CONSTRAINT case_dist_track_person_id_fkey FOREIGN KEY (person_id) REFERENCES public.case_person(person_id) ON DELETE CASCADE
);
CREATE INDEX case_dist_track_act_idx ON public.case_dist_track USING btree (act);
CREATE INDEX case_dist_track_active_idx ON public.case_dist_track USING btree (active);
CREATE INDEX case_dist_track_amount_idx ON public.case_dist_track USING btree (amount);
CREATE INDEX case_dist_track_ba_active_idx ON public.case_dist_track USING btree (ba_active);
CREATE INDEX case_dist_track_ba_distrainted_idx ON public.case_dist_track USING btree (ba_distrainted);
CREATE INDEX case_dist_track_ba_exists_idx ON public.case_dist_track USING btree (ba_exists);
CREATE INDEX case_dist_track_case_id_idx ON public.case_dist_track USING btree (case_id);
CREATE INDEX case_dist_track_created_idx ON public.case_dist_track USING btree (created);
CREATE INDEX case_dist_track_ld_confirmed_idx ON public.case_dist_track USING btree (ld_confirmed);
CREATE INDEX case_dist_track_ld_confirmed_inst_idx ON public.case_dist_track USING btree (ld_confirmed_inst);
CREATE INDEX case_dist_track_ld_confirmed_reg_idx ON public.case_dist_track USING btree (ld_confirmed_reg);
CREATE INDEX case_dist_track_ld_created_idx ON public.case_dist_track USING btree (ld_created);
CREATE INDEX case_dist_track_ld_printed_idx ON public.case_dist_track USING btree (ld_printed);
CREATE INDEX case_dist_track_ld_printed_inst_idx ON public.case_dist_track USING btree (ld_printed_inst);
CREATE INDEX case_dist_track_ld_printed_reg_idx ON public.case_dist_track USING btree (ld_printed_reg);
CREATE INDEX case_dist_track_ld_raise_confirmed_idx ON public.case_dist_track USING btree (ld_raise_confirmed);
CREATE INDEX case_dist_track_ld_raise_confirmed_inst_idx ON public.case_dist_track USING btree (ld_raise_confirmed_inst);
CREATE INDEX case_dist_track_ld_raise_confirmed_reg_idx ON public.case_dist_track USING btree (ld_raise_confirmed_reg);
CREATE INDEX case_dist_track_ld_raise_printed_idx ON public.case_dist_track USING btree (ld_raise_printed);
CREATE INDEX case_dist_track_ld_raise_printed_inst_idx ON public.case_dist_track USING btree (ld_raise_printed_inst);
CREATE INDEX case_dist_track_ld_raise_printed_reg_idx ON public.case_dist_track USING btree (ld_raise_printed_reg);
CREATE INDEX case_dist_track_ld_raise_replied_idx ON public.case_dist_track USING btree (ld_raise_replied);
CREATE INDEX case_dist_track_ld_raise_replied_inst_idx ON public.case_dist_track USING btree (ld_raise_replied_inst);
CREATE INDEX case_dist_track_ld_raise_replied_reg_idx ON public.case_dist_track USING btree (ld_raise_replied_reg);
CREATE INDEX case_dist_track_ld_raised_idx ON public.case_dist_track USING btree (ld_raised);
CREATE INDEX case_dist_track_ld_replied_idx ON public.case_dist_track USING btree (ld_replied);
CREATE INDEX case_dist_track_ld_replied_inst_idx ON public.case_dist_track USING btree (ld_replied_inst);
CREATE INDEX case_dist_track_ld_replied_reg_idx ON public.case_dist_track USING btree (ld_replied_reg);
CREATE INDEX case_dist_track_modified_idx ON public.case_dist_track USING btree (modified);
CREATE INDEX case_dist_track_person_id_idx ON public.case_dist_track USING btree (person_id);
CREATE INDEX case_dist_track_proc_ids_idx ON public.case_dist_track USING btree (proc_ids);
CREATE INDEX case_dist_track_ref_id_idx ON public.case_dist_track USING btree (ref_id);
CREATE INDEX case_dist_track_type_id_idx ON public.case_dist_track USING btree (type_id);


-- public.case_execution_list definition

-- Drop table

-- DROP TABLE public.case_execution_list;

CREATE TABLE public.case_execution_list (
	exec_id serial4 NOT NULL,
	case_id int4 NOT NULL,
	pretext int2 DEFAULT 0 NOT NULL,
	requisites jsonb DEFAULT '{}'::jsonb NOT NULL,
	created timestamptz DEFAULT now() NOT NULL,
	created_id int4 NOT NULL,
	modified timestamptz DEFAULT now() NOT NULL,
	modified_id int4 NOT NULL,
	amount_main numeric(12, 3) DEFAULT 0::numeric NOT NULL,
	contract_interest numeric(12, 3) DEFAULT 0::numeric NOT NULL,
	penalty_interest numeric(12, 3) DEFAULT 0::numeric NOT NULL,
	legal_expenses numeric(12, 3) DEFAULT 0::numeric NOT NULL,
	brokerage_expenses numeric(12, 3) DEFAULT 0::numeric NOT NULL,
	linterest timestamptz NULL,
	included bool DEFAULT false NOT NULL,
	brokerage numeric(12, 3) DEFAULT 0::numeric NOT NULL,
	civil_case_nr varchar(30) NULL,
	disabled timestamptz NULL,
	main_currency_id int2 DEFAULT 1 NOT NULL,
	legal_currency_id int2 DEFAULT 1 NOT NULL,
	brok_expense_currency_id int2 DEFAULT 1 NOT NULL,
	brok_currency_id int2 DEFAULT 1 NOT NULL,
	contract_interest_currency_id int2 DEFAULT 1 NOT NULL,
	penalty_interest_currency_id int2 DEFAULT 1 NOT NULL,
	CONSTRAINT case_execution_list_pkey PRIMARY KEY (exec_id),
	CONSTRAINT case_execution_list_case_id_fkey FOREIGN KEY (case_id) REFERENCES public.case_case(case_id) ON DELETE RESTRICT
);
CREATE INDEX case_execution_list_case_id_idx ON public.case_execution_list USING btree (case_id);
CREATE INDEX case_execution_list_civil_case_nr_idx ON public.case_execution_list USING btree (civil_case_nr);
CREATE INDEX case_execution_list_created_id_idx ON public.case_execution_list USING btree (created_id);
CREATE INDEX case_execution_list_created_idx ON public.case_execution_list USING btree (created);
CREATE INDEX case_execution_list_included_idx ON public.case_execution_list USING btree (included);
CREATE INDEX case_execution_list_linterest_idx ON public.case_execution_list USING btree (linterest);
CREATE INDEX case_execution_list_modified_id_idx ON public.case_execution_list USING btree (modified_id);
CREATE INDEX case_execution_list_modified_idx ON public.case_execution_list USING btree (modified);
CREATE INDEX case_execution_list_pretext_idx ON public.case_execution_list USING btree (pretext);
CREATE INDEX case_execution_list_requisites_idx ON public.case_execution_list USING btree (requisites);


-- public.case_ind_process definition

-- Drop table

-- DROP TABLE public.case_ind_process;

CREATE TABLE public.case_ind_process (
	p_id bigserial NOT NULL,
	person_id int8 NOT NULL,
	case_id int8 NULL,
	act_id int8 NOT NULL,
	created timestamptz DEFAULT now() NOT NULL,
	distrainted timestamptz NULL,
	distraint_deed_id int8 NULL,
	distraint_confirmed timestamptz NULL,
	raised timestamptz NULL,
	raise_confirmed timestamptz NULL,
	CONSTRAINT case_ind_process_pkey PRIMARY KEY (p_id),
	CONSTRAINT case_ind_process_act_id_fkey FOREIGN KEY (act_id) REFERENCES public.case_ind_active(act_id) ON DELETE RESTRICT,
	CONSTRAINT case_ind_process_case_id_fkey FOREIGN KEY (case_id) REFERENCES public.case_case(case_id) ON DELETE CASCADE,
	CONSTRAINT case_ind_process_distraint_deed_id_fkey FOREIGN KEY (distraint_deed_id) REFERENCES public.case_case_deed(deed_id) ON DELETE CASCADE,
	CONSTRAINT case_ind_process_person_id_fkey FOREIGN KEY (person_id) REFERENCES public.case_person(person_id) ON DELETE CASCADE
);
CREATE INDEX case_ind_process_act_id_idx ON public.case_ind_process USING btree (act_id);
CREATE INDEX case_ind_process_case_id_idx ON public.case_ind_process USING btree (case_id);
CREATE INDEX case_ind_process_created_idx ON public.case_ind_process USING btree (created);
CREATE INDEX case_ind_process_distraint_confirmed_idx ON public.case_ind_process USING btree (distraint_confirmed);
CREATE INDEX case_ind_process_distraint_deed_id_idx ON public.case_ind_process USING btree (distraint_deed_id);
CREATE INDEX case_ind_process_distrainted_idx ON public.case_ind_process USING btree (distrainted);
CREATE INDEX case_ind_process_person_id_idx ON public.case_ind_process USING btree (person_id);
CREATE INDEX case_ind_process_raise_confirmed_idx ON public.case_ind_process USING btree (raise_confirmed);
CREATE INDEX case_ind_process_raised_idx ON public.case_ind_process USING btree (raised);


-- public.case_loc_log definition

-- Drop table

-- DROP TABLE public.case_loc_log;

CREATE TABLE public.case_loc_log (
	rl_id bigserial NOT NULL,
	case_id int8 NOT NULL,
	stamp timestamptz DEFAULT now() NOT NULL,
	old_id int4 NULL,
	new_id int4 NULL,
	old_arch_id int4 NULL,
	old_arch_year int4 DEFAULT 2000 NOT NULL,
	old_arch_cont int4 DEFAULT 0 NOT NULL,
	new_arch_id int4 NULL,
	new_arch_year int4 DEFAULT 2000 NOT NULL,
	new_arch_cont int4 DEFAULT 0 NOT NULL,
	CONSTRAINT case_loc_log_pkey PRIMARY KEY (rl_id),
	CONSTRAINT case_loc_log_case_id_fkey FOREIGN KEY (case_id) REFERENCES public.case_case(case_id) ON DELETE CASCADE,
	CONSTRAINT case_loc_log_new_arch_id_fkey FOREIGN KEY (new_arch_id) REFERENCES public.case_archive(arch_id) ON DELETE SET NULL,
	CONSTRAINT case_loc_log_new_id FOREIGN KEY (new_id) REFERENCES public."user"(user_id) ON DELETE RESTRICT,
	CONSTRAINT case_loc_log_old_arch_id_fkey FOREIGN KEY (old_arch_id) REFERENCES public.case_archive(arch_id) ON DELETE SET NULL,
	CONSTRAINT case_loc_log_old_id FOREIGN KEY (old_id) REFERENCES public."user"(user_id) ON DELETE RESTRICT
);
CREATE INDEX case_loc_log_case_id_idx ON public.case_loc_log USING btree (case_id);
CREATE INDEX case_loc_log_stamp_idx ON public.case_loc_log USING btree (stamp);


-- public.case_movable_process definition

-- Drop table

-- DROP TABLE public.case_movable_process;

CREATE TABLE public.case_movable_process (
	p_id bigserial NOT NULL,
	person_id int8 NOT NULL,
	case_id int8 NULL,
	act_id int8 NOT NULL,
	created timestamptz DEFAULT now() NOT NULL,
	distrainted timestamptz NULL,
	distraint_deed_id int8 NULL,
	distraint_confirmed timestamptz NULL,
	raised timestamptz NULL,
	raise_confirmed timestamptz NULL,
	CONSTRAINT case_movable_process_pkey PRIMARY KEY (p_id),
	CONSTRAINT case_movable_process_act_id_fkey FOREIGN KEY (act_id) REFERENCES public.case_movable_actives(act_id) ON DELETE RESTRICT,
	CONSTRAINT case_movable_process_case_id_fkey FOREIGN KEY (case_id) REFERENCES public.case_case(case_id) ON DELETE CASCADE,
	CONSTRAINT case_movable_process_distraint_deed_id_fkey FOREIGN KEY (distraint_deed_id) REFERENCES public.case_case_deed(deed_id) ON DELETE CASCADE,
	CONSTRAINT case_movable_process_person_id_fkey FOREIGN KEY (person_id) REFERENCES public.case_person(person_id) ON DELETE CASCADE
);
CREATE INDEX case_movable_process_act_id_idx ON public.case_movable_process USING btree (act_id);
CREATE INDEX case_movable_process_case_id_idx ON public.case_movable_process USING btree (case_id);
CREATE INDEX case_movable_process_created_idx ON public.case_movable_process USING btree (created);
CREATE INDEX case_movable_process_distraint_confirmed_idx ON public.case_movable_process USING btree (distraint_confirmed);
CREATE INDEX case_movable_process_distraint_deed_id_idx ON public.case_movable_process USING btree (distraint_deed_id);
CREATE INDEX case_movable_process_distrainted_idx ON public.case_movable_process USING btree (distrainted);
CREATE INDEX case_movable_process_person_id_idx ON public.case_movable_process USING btree (person_id);
CREATE INDEX case_movable_process_raise_confirmed_idx ON public.case_movable_process USING btree (raise_confirmed);
CREATE INDEX case_movable_process_raised_idx ON public.case_movable_process USING btree (raised);


-- public.case_nap_kat_ent definition

-- Drop table

-- DROP TABLE public.case_nap_kat_ent;

CREATE TABLE public.case_nap_kat_ent (
	ent_id bigserial NOT NULL,
	case_id int8 NOT NULL,
	zone_id int4 NULL,
	ref_id int8 NULL,
	post varchar(10) NULL,
	CONSTRAINT case_nap_kat_ent_pkey PRIMARY KEY (ent_id),
	CONSTRAINT case_nap_kat_ent_case_id_fkey FOREIGN KEY (case_id) REFERENCES public.case_case(case_id) ON DELETE CASCADE
);
CREATE INDEX case_nap_kat_ent_case_id_idx ON public.case_nap_kat_ent USING btree (case_id);
CREATE INDEX case_nap_kat_ent_post_idx ON public.case_nap_kat_ent USING btree (post);
CREATE INDEX case_nap_kat_ent_ref_id_idx ON public.case_nap_kat_ent USING btree (ref_id);
CREATE INDEX case_nap_kat_ent_zone_id_idx ON public.case_nap_kat_ent USING btree (zone_id);


-- public.case_old_calc_params definition

-- Drop table

-- DROP TABLE public.case_old_calc_params;

CREATE TABLE public.case_old_calc_params (
	case_id int8 NOT NULL,
	enabled bool DEFAULT true NOT NULL,
	"data" jsonb DEFAULT '{}'::jsonb NOT NULL,
	CONSTRAINT case_old_calc_params_pkey PRIMARY KEY (case_id),
	CONSTRAINT case_old_calc_params_case_id_fkey FOREIGN KEY (case_id) REFERENCES public.case_case(case_id) ON DELETE CASCADE
);
CREATE INDEX case_old_calc_params_case_id_idx ON public.case_old_calc_params USING btree (case_id);


-- public.case_old_payment definition

-- Drop table

-- DROP TABLE public.case_old_payment;

CREATE TABLE public.case_old_payment (
	op_id bigserial NOT NULL,
	case_id int8 NOT NULL,
	stamp timestamptz DEFAULT now() NOT NULL,
	"comment" text NULL,
	amount numeric(12, 3) DEFAULT 0::numeric NOT NULL,
	currency_id int2 DEFAULT 1::smallint NOT NULL,
	CONSTRAINT case_old_payment_pkey PRIMARY KEY (op_id),
	CONSTRAINT case_old_payment_case_id_fkey FOREIGN KEY (case_id) REFERENCES public.case_case(case_id) ON DELETE CASCADE
);
CREATE INDEX case_old_payment_case_id_idx ON public.case_old_payment USING btree (case_id);
CREATE INDEX case_old_payment_stamp_idx ON public.case_old_payment USING btree (stamp);


-- public.case_other_amount definition

-- Drop table

-- DROP TABLE public.case_other_amount;

CREATE TABLE public.case_other_amount (
	aid serial4 NOT NULL,
	exec_id int4 NOT NULL,
	amount numeric(12, 3) DEFAULT 0 NOT NULL,
	currency_id int2 DEFAULT 1 NOT NULL,
	note varchar(80) NULL,
	tfrom timestamptz NULL,
	CONSTRAINT case_other_amount_pkey PRIMARY KEY (aid),
	CONSTRAINT case_other_amount_exec_id_fkey FOREIGN KEY (exec_id) REFERENCES public.case_execution_list(exec_id) ON DELETE CASCADE
);
CREATE INDEX case_other_amount_exec_id_idx ON public.case_other_amount USING btree (exec_id);
CREATE INDEX case_other_amount_tfrom_idx ON public.case_other_amount USING btree (tfrom);


-- public.case_out_money definition

-- Drop table

-- DROP TABLE public.case_out_money;

CREATE TABLE public.case_out_money (
	money_id serial4 NOT NULL,
	person_id int4 NULL,
	case_id int4 NOT NULL,
	amount numeric(12, 3) DEFAULT 0 NOT NULL,
	currency_id int4 DEFAULT 1 NOT NULL,
	bank_id int2 DEFAULT '-1'::integer NOT NULL,
	bank_account varchar(90) NULL,
	created timestamptz DEFAULT now() NOT NULL,
	last_input timestamptz DEFAULT now() NOT NULL,
	sent timestamptz NULL,
	from_bank_id int2 DEFAULT '-1'::integer NOT NULL,
	from_iban varchar(90) NULL,
	sent_id int4 NULL,
	debtor_id int4 NULL,
	reason text NULL,
	add_data jsonb DEFAULT '{}'::jsonb NOT NULL,
	taxed bool DEFAULT false NOT NULL,
	CONSTRAINT case_out_money_pkey PRIMARY KEY (money_id),
	CONSTRAINT case_out_money_case_id_fkey FOREIGN KEY (case_id) REFERENCES public.case_case(case_id) ON DELETE RESTRICT,
	CONSTRAINT case_out_money_debtor_id_fkey FOREIGN KEY (debtor_id) REFERENCES public.case_person(person_id) ON DELETE RESTRICT,
	CONSTRAINT case_out_money_person_id_fkey FOREIGN KEY (person_id) REFERENCES public.case_person(person_id) ON DELETE RESTRICT
);
CREATE INDEX case_out_money_amount_idx ON public.case_out_money USING btree (amount);
CREATE INDEX case_out_money_bank_id_idx ON public.case_out_money USING btree (bank_id);
CREATE INDEX case_out_money_case_id_idx ON public.case_out_money USING btree (case_id);
CREATE INDEX case_out_money_created_idx ON public.case_out_money USING btree (created);
CREATE INDEX case_out_money_debtor_id_idx ON public.case_out_money USING btree (debtor_id);
CREATE INDEX case_out_money_from_bank_id_idx ON public.case_out_money USING btree (from_bank_id);
CREATE INDEX case_out_money_from_iban_idx ON public.case_out_money USING btree (from_iban);
CREATE INDEX case_out_money_last_input_idx ON public.case_out_money USING btree (last_input);
CREATE INDEX case_out_money_person_id_idx ON public.case_out_money USING btree (person_id);
CREATE INDEX case_out_money_reason_idx ON public.case_out_money USING btree (reason);
CREATE INDEX case_out_money_sent_id_idx ON public.case_out_money USING btree (sent_id);
CREATE INDEX case_out_money_sent_idx ON public.case_out_money USING btree (sent);
CREATE INDEX case_out_money_taxed_idx ON public.case_out_money USING btree (taxed);


-- public.case_paper_process definition

-- Drop table

-- DROP TABLE public.case_paper_process;

CREATE TABLE public.case_paper_process (
	p_id bigserial NOT NULL,
	person_id int8 NOT NULL,
	case_id int8 NULL,
	act_id int8 NOT NULL,
	created timestamptz DEFAULT now() NOT NULL,
	distrainted timestamptz NULL,
	distraint_deed_id int8 NULL,
	distraint_confirmed timestamptz NULL,
	raised timestamptz NULL,
	raise_confirmed timestamptz NULL,
	CONSTRAINT case_paper_process_pkey PRIMARY KEY (p_id),
	CONSTRAINT case_paper_process_act_id_fkey FOREIGN KEY (act_id) REFERENCES public.case_paper_actives(act_id) ON DELETE RESTRICT,
	CONSTRAINT case_paper_process_case_id_fkey FOREIGN KEY (case_id) REFERENCES public.case_case(case_id) ON DELETE CASCADE,
	CONSTRAINT case_paper_process_distraint_deed_id_fkey FOREIGN KEY (distraint_deed_id) REFERENCES public.case_case_deed(deed_id) ON DELETE CASCADE,
	CONSTRAINT case_paper_process_person_id_fkey FOREIGN KEY (person_id) REFERENCES public.case_person(person_id) ON DELETE CASCADE
);
CREATE INDEX case_paper_process_act_id_idx ON public.case_paper_process USING btree (act_id);
CREATE INDEX case_paper_process_case_id_idx ON public.case_paper_process USING btree (case_id);
CREATE INDEX case_paper_process_created_idx ON public.case_paper_process USING btree (created);
CREATE INDEX case_paper_process_distraint_confirmed_idx ON public.case_paper_process USING btree (distraint_confirmed);
CREATE INDEX case_paper_process_distraint_deed_id_idx ON public.case_paper_process USING btree (distraint_deed_id);
CREATE INDEX case_paper_process_distrainted_idx ON public.case_paper_process USING btree (distrainted);
CREATE INDEX case_paper_process_person_id_idx ON public.case_paper_process USING btree (person_id);
CREATE INDEX case_paper_process_raise_confirmed_idx ON public.case_paper_process USING btree (raise_confirmed);
CREATE INDEX case_paper_process_raised_idx ON public.case_paper_process USING btree (raised);


-- public.case_person_vehicle definition

-- Drop table

-- DROP TABLE public.case_person_vehicle;

CREATE TABLE public.case_person_vehicle (
	vhcl_id serial4 NOT NULL,
	person_id int4 NOT NULL,
	reg_num varchar(20) NULL,
	vhcl_num varchar(60) NULL,
	reg_date timestamptz NULL,
	town_id int4 NULL,
	actual bool DEFAULT true NOT NULL,
	created timestamptz NULL,
	created_id int4 NULL,
	modified timestamptz NULL,
	modified_id int4 NULL,
	brand_id int4 NULL,
	model_id int4 NULL,
	check_req timestamptz NULL,
	check_resp timestamptz NULL,
	reg_data jsonb NULL,
	CONSTRAINT case_person_vehicle_pkey PRIMARY KEY (vhcl_id),
	CONSTRAINT case_person_vehicle_brand_id_fkey FOREIGN KEY (brand_id) REFERENCES public.case_vhcl_brand(brand_id) ON DELETE SET NULL,
	CONSTRAINT case_person_vehicle_model_id_fkey FOREIGN KEY (model_id) REFERENCES public.case_vhcl_model(model_id) ON DELETE SET NULL,
	CONSTRAINT case_person_vehicle_person_id_fkey FOREIGN KEY (person_id) REFERENCES public.case_person(person_id) ON DELETE CASCADE,
	CONSTRAINT case_person_vehicle_town_id_fkey FOREIGN KEY (town_id) REFERENCES public.case_town(town_id) ON DELETE RESTRICT
);
CREATE INDEX case_person_vehicle_actual_idx ON public.case_person_vehicle USING btree (actual);
CREATE INDEX case_person_vehicle_brand_id_idx ON public.case_person_vehicle USING btree (brand_id);
CREATE INDEX case_person_vehicle_check_req_idx ON public.case_person_vehicle USING btree (check_req);
CREATE INDEX case_person_vehicle_check_resp_idx ON public.case_person_vehicle USING btree (check_resp);
CREATE INDEX case_person_vehicle_model_id_idx ON public.case_person_vehicle USING btree (model_id);
CREATE INDEX case_person_vehicle_person_id_idx ON public.case_person_vehicle USING btree (person_id);
CREATE INDEX case_person_vehicle_town_id_idx ON public.case_person_vehicle USING btree (town_id);


-- public.case_pns_process definition

-- Drop table

-- DROP TABLE public.case_pns_process;

CREATE TABLE public.case_pns_process (
	pp_id bigserial NOT NULL,
	person_id int8 NOT NULL,
	case_id int8 NOT NULL,
	created timestamptz DEFAULT now() NOT NULL,
	create_deed_id int8 NOT NULL,
	pns_id int8 NULL,
	distrainted timestamptz NULL,
	distraint_deed_id int8 NULL,
	distraint_confirmed timestamptz NULL,
	raised timestamptz NULL,
	raise_confirmed timestamptz NULL,
	CONSTRAINT case_pns_process_pkey PRIMARY KEY (pp_id),
	CONSTRAINT case_pns_process_case_id_fkey FOREIGN KEY (case_id) REFERENCES public.case_case(case_id) ON DELETE RESTRICT,
	CONSTRAINT case_pns_process_create_deed_id_fkey FOREIGN KEY (create_deed_id) REFERENCES public.case_case_deed(deed_id) ON DELETE CASCADE,
	CONSTRAINT case_pns_process_distraint_deed_id_fkey FOREIGN KEY (distraint_deed_id) REFERENCES public.case_case_deed(deed_id) ON DELETE CASCADE,
	CONSTRAINT case_pns_process_person_id_fkey FOREIGN KEY (person_id) REFERENCES public.case_person(person_id) ON DELETE RESTRICT,
	CONSTRAINT case_pns_process_pns_id_fkey FOREIGN KEY (pns_id) REFERENCES public.case_pension(pid) ON DELETE SET NULL
);
CREATE INDEX case_pns_process_case_id_idx ON public.case_pns_process USING btree (case_id);
CREATE INDEX case_pns_process_create_deed_id_idx ON public.case_pns_process USING btree (create_deed_id);
CREATE INDEX case_pns_process_created_idx ON public.case_pns_process USING btree (created);
CREATE INDEX case_pns_process_distraint_confirmed_idx ON public.case_pns_process USING btree (distraint_confirmed);
CREATE INDEX case_pns_process_distraint_deed_id_idx ON public.case_pns_process USING btree (distraint_deed_id);
CREATE INDEX case_pns_process_distrainted_idx ON public.case_pns_process USING btree (distrainted);
CREATE INDEX case_pns_process_person_id_idx ON public.case_pns_process USING btree (person_id);
CREATE INDEX case_pns_process_pns_id_idx ON public.case_pns_process USING btree (pns_id);
CREATE INDEX case_pns_process_raise_confirmed_idx ON public.case_pns_process USING btree (raise_confirmed);
CREATE INDEX case_pns_process_raised_idx ON public.case_pns_process USING btree (raised);


-- public.case_pof_process definition

-- Drop table

-- DROP TABLE public.case_pof_process;

CREATE TABLE public.case_pof_process (
	p_id bigserial NOT NULL,
	person_id int8 NOT NULL,
	case_id int8 NULL,
	act_id int8 NOT NULL,
	created timestamptz DEFAULT now() NOT NULL,
	distrainted timestamptz NULL,
	distraint_deed_id int8 NULL,
	distraint_confirmed timestamptz NULL,
	raised timestamptz NULL,
	raise_confirmed timestamptz NULL,
	CONSTRAINT case_pof_process_pkey PRIMARY KEY (p_id),
	CONSTRAINT case_pof_process_act_id_fkey FOREIGN KEY (act_id) REFERENCES public.case_part_of_factory(act_id) ON DELETE RESTRICT,
	CONSTRAINT case_pof_process_case_id_fkey FOREIGN KEY (case_id) REFERENCES public.case_case(case_id) ON DELETE CASCADE,
	CONSTRAINT case_pof_process_distraint_deed_id_fkey FOREIGN KEY (distraint_deed_id) REFERENCES public.case_case_deed(deed_id) ON DELETE CASCADE,
	CONSTRAINT case_pof_process_person_id_fkey FOREIGN KEY (person_id) REFERENCES public.case_person(person_id) ON DELETE CASCADE
);
CREATE INDEX case_pof_process_act_id_idx ON public.case_pof_process USING btree (act_id);
CREATE INDEX case_pof_process_case_id_idx ON public.case_pof_process USING btree (case_id);
CREATE INDEX case_pof_process_created_idx ON public.case_pof_process USING btree (created);
CREATE INDEX case_pof_process_distraint_confirmed_idx ON public.case_pof_process USING btree (distraint_confirmed);
CREATE INDEX case_pof_process_distraint_deed_id_idx ON public.case_pof_process USING btree (distraint_deed_id);
CREATE INDEX case_pof_process_distrainted_idx ON public.case_pof_process USING btree (distrainted);
CREATE INDEX case_pof_process_person_id_idx ON public.case_pof_process USING btree (person_id);
CREATE INDEX case_pof_process_raise_confirmed_idx ON public.case_pof_process USING btree (raise_confirmed);
CREATE INDEX case_pof_process_raised_idx ON public.case_pof_process USING btree (raised);


-- public.case_pub_sale definition

-- Drop table

-- DROP TABLE public.case_pub_sale;

CREATE TABLE public.case_pub_sale (
	ps_id bigserial NOT NULL,
	case_id int8 NOT NULL,
	person_id int8 NOT NULL,
	tfrom timestamptz DEFAULT now() NOT NULL,
	open_offers timestamptz DEFAULT now() NOT NULL,
	status_date timestamptz NULL,
	status int2 DEFAULT 0::smallint NOT NULL,
	"name" text NULL,
	CONSTRAINT case_pub_sale_pkey PRIMARY KEY (ps_id),
	CONSTRAINT case_pub_sale_case_id_fkey FOREIGN KEY (case_id) REFERENCES public.case_case(case_id) ON DELETE CASCADE,
	CONSTRAINT case_pub_sale_person_id_fkey FOREIGN KEY (person_id) REFERENCES public.case_person(person_id) ON DELETE CASCADE
);
CREATE INDEX case_pub_sale_case_id_idx ON public.case_pub_sale USING btree (case_id);
CREATE INDEX case_pub_sale_person_id_idx ON public.case_pub_sale USING btree (person_id);
CREATE INDEX case_pub_sale_status_date_idx ON public.case_pub_sale USING btree (status_date);
CREATE INDEX case_pub_sale_status_idx ON public.case_pub_sale USING btree (status);
CREATE INDEX case_pub_sale_tfrom_idx ON public.case_pub_sale USING btree (tfrom);


-- public.case_quarter definition

-- Drop table

-- DROP TABLE public.case_quarter;

CREATE TABLE public.case_quarter (
	qtr_id serial4 NOT NULL,
	country_id int4 NOT NULL,
	zone_id int4 NOT NULL,
	mun_id int4 NOT NULL,
	town_id int4 NOT NULL,
	"name" varchar(120) NOT NULL,
	post int4 NULL,
	CONSTRAINT case_quarter_pkey PRIMARY KEY (qtr_id),
	CONSTRAINT case_quarter_mun_id_fkey FOREIGN KEY (mun_id) REFERENCES public.case_municipality(mun_id) ON DELETE RESTRICT,
	CONSTRAINT case_quarter_town_id_fkey FOREIGN KEY (town_id) REFERENCES public.case_town(town_id) ON DELETE RESTRICT
);
CREATE INDEX case_quarter_country_id_idx ON public.case_quarter USING btree (country_id);
CREATE INDEX case_quarter_mun_id_idx ON public.case_quarter USING btree (mun_id);
CREATE INDEX case_quarter_name_idx ON public.case_quarter USING btree (name);
CREATE INDEX case_quarter_post_idx ON public.case_quarter USING btree (post);
CREATE INDEX case_quarter_town_id_idx ON public.case_quarter USING btree (town_id);
CREATE INDEX case_quarter_zone_id_idx ON public.case_quarter USING btree (zone_id);


-- public.case_re_process definition

-- Drop table

-- DROP TABLE public.case_re_process;

CREATE TABLE public.case_re_process (
	p_id bigserial NOT NULL,
	person_id int8 NOT NULL,
	case_id int8 NOT NULL,
	created timestamptz DEFAULT now() NOT NULL,
	re_id int8 NULL,
	distrainted timestamptz NULL,
	distraint_deed_id int8 NULL,
	distraint_confirmed timestamptz NULL,
	raised timestamptz NULL,
	raise_confirmed timestamptz NULL,
	CONSTRAINT case_re_process_pkey PRIMARY KEY (p_id),
	CONSTRAINT case_re_process_case_id_fkey FOREIGN KEY (case_id) REFERENCES public.case_case(case_id) ON DELETE RESTRICT,
	CONSTRAINT case_re_process_distraint_deed_id_fkey FOREIGN KEY (distraint_deed_id) REFERENCES public.case_case_deed(deed_id) ON DELETE CASCADE,
	CONSTRAINT case_re_process_person_id_fkey FOREIGN KEY (person_id) REFERENCES public.case_person(person_id) ON DELETE RESTRICT,
	CONSTRAINT case_re_process_re_id_fkey FOREIGN KEY (re_id) REFERENCES public.case_real_estate(rs_id) ON DELETE SET NULL
);
CREATE INDEX case_re_process_case_id_idx ON public.case_re_process USING btree (case_id);
CREATE INDEX case_re_process_created_idx ON public.case_re_process USING btree (created);
CREATE INDEX case_re_process_distraint_confirmed_idx ON public.case_re_process USING btree (distraint_confirmed);
CREATE INDEX case_re_process_distraint_deed_id_idx ON public.case_re_process USING btree (distraint_deed_id);
CREATE INDEX case_re_process_distrainted_idx ON public.case_re_process USING btree (distrainted);
CREATE INDEX case_re_process_person_id_idx ON public.case_re_process USING btree (person_id);
CREATE INDEX case_re_process_raise_confirmed_idx ON public.case_re_process USING btree (raise_confirmed);
CREATE INDEX case_re_process_raised_idx ON public.case_re_process USING btree (raised);
CREATE INDEX case_re_process_re_id_idx ON public.case_re_process USING btree (re_id);


-- public.case_resp_log definition

-- Drop table

-- DROP TABLE public.case_resp_log;

CREATE TABLE public.case_resp_log (
	rl_id bigserial NOT NULL,
	case_id int8 NOT NULL,
	stamp timestamptz DEFAULT now() NOT NULL,
	responsible jsonb DEFAULT '[]'::jsonb NOT NULL,
	CONSTRAINT case_resp_log_pkey PRIMARY KEY (rl_id),
	CONSTRAINT case_resp_log_case_id_fkey FOREIGN KEY (case_id) REFERENCES public.case_case(case_id) ON DELETE CASCADE
);
CREATE INDEX case_resp_log_case_id_idx ON public.case_resp_log USING btree (case_id);
CREATE INDEX case_resp_log_responsible_idx ON public.case_resp_log USING btree (responsible);
CREATE INDEX case_resp_log_stamp_idx ON public.case_resp_log USING btree (stamp);


-- public.case_task definition

-- Drop table

-- DROP TABLE public.case_task;

CREATE TABLE public.case_task (
	task_id bigserial NOT NULL,
	case_id int8 NOT NULL,
	tstart timestamptz DEFAULT now() NOT NULL,
	tend timestamptz DEFAULT now() NOT NULL,
	descr text NULL,
	state int2 DEFAULT '0'::smallint NOT NULL,
	involved jsonb DEFAULT '[]'::jsonb NOT NULL,
	event_ids jsonb DEFAULT '[]'::jsonb NOT NULL,
	type_id int4 NULL,
	CONSTRAINT case_task_pkey PRIMARY KEY (task_id),
	CONSTRAINT case_task_case_id_fkey FOREIGN KEY (case_id) REFERENCES public.case_case(case_id) ON DELETE CASCADE,
	CONSTRAINT case_task_type_id_fkey FOREIGN KEY (type_id) REFERENCES public.case_task_type(type_id) ON DELETE RESTRICT
);
CREATE INDEX case_task_case_id_idx ON public.case_task USING btree (case_id);
CREATE INDEX case_task_involved_idx ON public.case_task USING btree (involved);
CREATE INDEX case_task_state_idx ON public.case_task USING btree (state);
CREATE INDEX case_task_tend_idx ON public.case_task USING btree (tend);
CREATE INDEX case_task_tstart_idx ON public.case_task USING btree (tstart);
CREATE INDEX case_task_type_id_idx ON public.case_task USING btree (type_id);


-- public.case_tech_process definition

-- Drop table

-- DROP TABLE public.case_tech_process;

CREATE TABLE public.case_tech_process (
	p_id bigserial NOT NULL,
	person_id int8 NOT NULL,
	case_id int8 NULL,
	tech_id int8 NOT NULL,
	created timestamptz DEFAULT now() NOT NULL,
	distrainted timestamptz NULL,
	distraint_deed_id int8 NULL,
	distraint_confirmed timestamptz NULL,
	raised timestamptz NULL,
	raise_confirmed timestamptz NULL,
	CONSTRAINT case_tech_process_pkey PRIMARY KEY (p_id),
	CONSTRAINT case_tech_process_case_id_fkey FOREIGN KEY (case_id) REFERENCES public.case_case(case_id) ON DELETE CASCADE,
	CONSTRAINT case_tech_process_distraint_deed_id_fkey FOREIGN KEY (distraint_deed_id) REFERENCES public.case_case_deed(deed_id) ON DELETE CASCADE,
	CONSTRAINT case_tech_process_person_id_fkey FOREIGN KEY (person_id) REFERENCES public.case_person(person_id) ON DELETE CASCADE,
	CONSTRAINT case_tech_process_tech_id_fkey FOREIGN KEY (tech_id) REFERENCES public.case_tech_matter(tech_id) ON DELETE RESTRICT
);
CREATE INDEX case_tech_process_case_id_idx ON public.case_tech_process USING btree (case_id);
CREATE INDEX case_tech_process_created_idx ON public.case_tech_process USING btree (created);
CREATE INDEX case_tech_process_distraint_confirmed_idx ON public.case_tech_process USING btree (distraint_confirmed);
CREATE INDEX case_tech_process_distraint_deed_id_idx ON public.case_tech_process USING btree (distraint_deed_id);
CREATE INDEX case_tech_process_distrainted_idx ON public.case_tech_process USING btree (distrainted);
CREATE INDEX case_tech_process_person_id_idx ON public.case_tech_process USING btree (person_id);
CREATE INDEX case_tech_process_raise_confirmed_idx ON public.case_tech_process USING btree (raise_confirmed);
CREATE INDEX case_tech_process_raised_idx ON public.case_tech_process USING btree (raised);
CREATE INDEX case_tech_process_tech_id_idx ON public.case_tech_process USING btree (tech_id);


-- public.case_vehicle_process definition

-- Drop table

-- DROP TABLE public.case_vehicle_process;

CREATE TABLE public.case_vehicle_process (
	p_id bigserial NOT NULL,
	person_id int8 NOT NULL,
	case_id int8 NULL,
	vhcl_id int8 NOT NULL,
	created timestamptz DEFAULT now() NOT NULL,
	distrainted timestamptz NULL,
	distraint_deed_id int8 NULL,
	distraint_confirmed timestamptz NULL,
	raised timestamptz NULL,
	raise_confirmed timestamptz NULL,
	CONSTRAINT case_vehicle_process_pkey PRIMARY KEY (p_id),
	CONSTRAINT case_vehicle_process_case_id_fkey FOREIGN KEY (case_id) REFERENCES public.case_case(case_id) ON DELETE CASCADE,
	CONSTRAINT case_vehicle_process_distraint_deed_id_fkey FOREIGN KEY (distraint_deed_id) REFERENCES public.case_case_deed(deed_id) ON DELETE CASCADE,
	CONSTRAINT case_vehicle_process_person_id_fkey FOREIGN KEY (person_id) REFERENCES public.case_person(person_id) ON DELETE CASCADE,
	CONSTRAINT case_vehicle_process_vhcl_id_fkey FOREIGN KEY (vhcl_id) REFERENCES public.case_person_vehicle(vhcl_id) ON DELETE RESTRICT
);
CREATE INDEX case_vehicle_process_case_id_idx ON public.case_vehicle_process USING btree (case_id);
CREATE INDEX case_vehicle_process_created_idx ON public.case_vehicle_process USING btree (created);
CREATE INDEX case_vehicle_process_distraint_confirmed_idx ON public.case_vehicle_process USING btree (distraint_confirmed);
CREATE INDEX case_vehicle_process_distraint_deed_id_idx ON public.case_vehicle_process USING btree (distraint_deed_id);
CREATE INDEX case_vehicle_process_distrainted_idx ON public.case_vehicle_process USING btree (distrainted);
CREATE INDEX case_vehicle_process_person_id_idx ON public.case_vehicle_process USING btree (person_id);
CREATE INDEX case_vehicle_process_raise_confirmed_idx ON public.case_vehicle_process USING btree (raise_confirmed);
CREATE INDEX case_vehicle_process_raised_idx ON public.case_vehicle_process USING btree (raised);
CREATE INDEX case_vehicle_process_vhcl_id_idx ON public.case_vehicle_process USING btree (vhcl_id);


-- public.case_wc_process definition

-- Drop table

-- DROP TABLE public.case_wc_process;

CREATE TABLE public.case_wc_process (
	wp_id bigserial NOT NULL,
	person_id int8 NOT NULL,
	case_id int8 NOT NULL,
	created timestamptz DEFAULT now() NOT NULL,
	create_deed_id int8 NOT NULL,
	wc_id int8 NULL,
	distrainted timestamptz NULL,
	distraint_deed_id int8 NULL,
	distraint_confirmed timestamptz NULL,
	raised timestamptz NULL,
	raise_confirmed timestamptz NULL,
	CONSTRAINT case_wc_process_pkey PRIMARY KEY (wp_id),
	CONSTRAINT case_wc_process_case_id_fkey FOREIGN KEY (case_id) REFERENCES public.case_case(case_id) ON DELETE RESTRICT,
	CONSTRAINT case_wc_process_create_deed_id_fkey FOREIGN KEY (create_deed_id) REFERENCES public.case_case_deed(deed_id) ON DELETE CASCADE,
	CONSTRAINT case_wc_process_distraint_deed_id_fkey FOREIGN KEY (distraint_deed_id) REFERENCES public.case_case_deed(deed_id) ON DELETE CASCADE,
	CONSTRAINT case_wc_process_person_id_fkey FOREIGN KEY (person_id) REFERENCES public.case_person(person_id) ON DELETE RESTRICT,
	CONSTRAINT case_wc_process_wc_id_fkey FOREIGN KEY (wc_id) REFERENCES public.case_work_contract(wc_id) ON DELETE SET NULL
);
CREATE INDEX case_wc_process_case_id_idx ON public.case_wc_process USING btree (case_id);
CREATE INDEX case_wc_process_create_deed_id_idx ON public.case_wc_process USING btree (create_deed_id);
CREATE INDEX case_wc_process_created_idx ON public.case_wc_process USING btree (created);
CREATE INDEX case_wc_process_distraint_confirmed_idx ON public.case_wc_process USING btree (distraint_confirmed);
CREATE INDEX case_wc_process_distraint_deed_id_idx ON public.case_wc_process USING btree (distraint_deed_id);
CREATE INDEX case_wc_process_distrainted_idx ON public.case_wc_process USING btree (distrainted);
CREATE INDEX case_wc_process_person_id_idx ON public.case_wc_process USING btree (person_id);
CREATE INDEX case_wc_process_raise_confirmed_idx ON public.case_wc_process USING btree (raise_confirmed);
CREATE INDEX case_wc_process_raised_idx ON public.case_wc_process USING btree (raised);
CREATE INDEX case_wc_process_wc_id_idx ON public.case_wc_process USING btree (wc_id);


-- public.cases_offspring definition

-- Drop table

-- DROP TABLE public.cases_offspring;

CREATE TABLE public.cases_offspring (
	att_id int8 NOT NULL,
	case_id int8 NOT NULL,
	stamp timestamptz NULL,
	old_debtor_id int8 NOT NULL,
	new_debtor_id int8 NOT NULL,
	exec_id int8 NULL,
	approved bool DEFAULT false NOT NULL,
	user_id int4 NULL,
	new_debtor_more jsonb DEFAULT '[]'::jsonb NOT NULL,
	CONSTRAINT cases_offspring_pkey PRIMARY KEY (att_id),
	CONSTRAINT cases_offspring_case_id_fkey FOREIGN KEY (case_id) REFERENCES public.case_case(case_id) ON DELETE CASCADE,
	CONSTRAINT cases_offspring_exec_id_fkey FOREIGN KEY (exec_id) REFERENCES public.case_execution_list(exec_id) ON DELETE CASCADE,
	CONSTRAINT cases_offspring_new_debtor_id_fkey FOREIGN KEY (new_debtor_id) REFERENCES public.case_person(person_id) ON DELETE RESTRICT,
	CONSTRAINT cases_offspring_old_debtor_id_fkey FOREIGN KEY (old_debtor_id) REFERENCES public.case_person(person_id) ON DELETE RESTRICT
);
CREATE INDEX cases_offspring_approved_idx ON public.cases_offspring USING btree (approved);
CREATE INDEX cases_offspring_case_id_idx ON public.cases_offspring USING btree (case_id);
CREATE INDEX cases_offspring_exec_id_idx ON public.cases_offspring USING btree (exec_id);
CREATE INDEX cases_offspring_new_debtor_id_idx ON public.cases_offspring USING btree (new_debtor_id);
CREATE INDEX cases_offspring_new_debtor_more_idx ON public.cases_offspring USING btree (new_debtor_more);
CREATE INDEX cases_offspring_old_debtor_id_idx ON public.cases_offspring USING btree (old_debtor_id);
CREATE INDEX cases_offspring_stamp_idx ON public.cases_offspring USING btree (stamp);


-- public.cases_person_comm definition

-- Drop table

-- DROP TABLE public.cases_person_comm;

CREATE TABLE public.cases_person_comm (
	com_id serial4 NOT NULL,
	case_id int4 NOT NULL,
	person_id int4 NOT NULL,
	mtype int2 DEFAULT 0 NOT NULL,
	stype int2 DEFAULT 0 NOT NULL,
	stamp timestamptz DEFAULT now() NOT NULL,
	pdata jsonb DEFAULT '{}'::jsonb NOT NULL,
	CONSTRAINT cases_person_comm_pkey PRIMARY KEY (com_id),
	CONSTRAINT cases_person_comm_case_id_fkey FOREIGN KEY (case_id) REFERENCES public.case_case(case_id) ON DELETE RESTRICT,
	CONSTRAINT cases_person_comm_person_id_fkey FOREIGN KEY (person_id) REFERENCES public.case_person(person_id) ON DELETE CASCADE
);
CREATE INDEX cases_person_comm_case_id_idx ON public.cases_person_comm USING btree (case_id);
CREATE INDEX cases_person_comm_mtype_idx ON public.cases_person_comm USING btree (mtype);
CREATE INDEX cases_person_comm_person_id_idx ON public.cases_person_comm USING btree (person_id);
CREATE INDEX cases_person_comm_stamp_idx ON public.cases_person_comm USING btree (stamp);
CREATE INDEX cases_person_comm_stype_idx ON public.cases_person_comm USING btree (stype);


-- public.chat_membership definition

-- Drop table

-- DROP TABLE public.chat_membership;

CREATE TABLE public.chat_membership (
	chat_membership_id serial4 NOT NULL,
	chat_room_id int4 NOT NULL,
	user_id int4 NOT NULL,
	timestamp_join timestamptz DEFAULT now() NOT NULL,
	timestamp_leave timestamptz NULL,
	chat_message_seen_id int4 NULL,
	CONSTRAINT chat_membership_chat_room_id_user_id_key UNIQUE (chat_room_id, user_id),
	CONSTRAINT chat_membership_pkey PRIMARY KEY (chat_membership_id),
	CONSTRAINT chat_membership_chat_room_id_fkey FOREIGN KEY (chat_room_id) REFERENCES public.chat_room(chat_room_id) ON DELETE RESTRICT,
	CONSTRAINT chat_membership_user_id_fkey FOREIGN KEY (user_id) REFERENCES public."user"(user_id) ON DELETE RESTRICT
);
CREATE INDEX chat_membership_chat_message_seen_id_idx ON public.chat_membership USING btree (chat_message_seen_id);
CREATE INDEX chat_membership_chat_room_id_idx ON public.chat_membership USING btree (chat_room_id);
CREATE INDEX chat_membership_user_id_idx ON public.chat_membership USING btree (user_id);


-- public.chat_message definition

-- Drop table

-- DROP TABLE public.chat_message;

CREATE TABLE public.chat_message (
	chat_message_id bigserial NOT NULL,
	chat_room_id int4 NOT NULL,
	user_id int4 NOT NULL,
	"timestamp" timestamptz DEFAULT now() NOT NULL,
	"content" text NULL,
	edited bool DEFAULT false NOT NULL,
	"type" int4 NULL,
	CONSTRAINT chat_message_pkey PRIMARY KEY (chat_message_id),
	CONSTRAINT chat_message_chat_room_id_fkey FOREIGN KEY (chat_room_id) REFERENCES public.chat_room(chat_room_id) ON DELETE RESTRICT,
	CONSTRAINT chat_message_user_id_fkey FOREIGN KEY (user_id) REFERENCES public."user"(user_id) ON DELETE RESTRICT
);
CREATE INDEX chat_message_chat_room_id_idx ON public.chat_message USING btree (chat_room_id);
CREATE INDEX chat_message_edited_idx ON public.chat_message USING btree (edited);
CREATE INDEX chat_message_timestamp_idx ON public.chat_message USING btree ("timestamp");
CREATE INDEX chat_message_timestamp_idx1 ON public.chat_message USING btree ("timestamp");
CREATE INDEX chat_message_user_id_idx ON public.chat_message USING btree (user_id);
CREATE INDEX chat_message_user_id_idx1 ON public.chat_message USING btree (user_id);


-- public.cl_expense definition

-- Drop table

-- DROP TABLE public.cl_expense;

CREATE TABLE public.cl_expense (
	expense_id serial4 NOT NULL,
	type_id int4 NOT NULL,
	starts_on timestamptz NOT NULL,
	ends_on timestamptz NOT NULL,
	repeat_type int4 NULL,
	end_after int4 NULL,
	repeat_every int4 NULL,
	amount numeric(11, 2) NULL,
	currency_id int4 NULL,
	descr varchar(90) NULL,
	"locked" bool DEFAULT false NOT NULL,
	locked_id int4 NULL,
	modified_id int4 NULL,
	notify_ids jsonb DEFAULT '[]'::jsonb NOT NULL,
	event_ids jsonb DEFAULT '[]'::jsonb NOT NULL,
	lock_notify_ids jsonb DEFAULT '[]'::jsonb NOT NULL,
	company_id int4 NOT NULL,
	state int4 DEFAULT 0 NOT NULL,
	library_id int4 NULL,
	CONSTRAINT cl_expense_pkey PRIMARY KEY (expense_id),
	CONSTRAINT cl_expense_company_id FOREIGN KEY (company_id) REFERENCES public.company(company_id) ON DELETE RESTRICT,
	CONSTRAINT cl_expense_type_id_fkey FOREIGN KEY (type_id) REFERENCES public.cl_expense_type(type_id)
);
CREATE INDEX cl_expense_company ON public.cl_expense USING btree (company_id);
CREATE INDEX cl_expense_descr ON public.cl_expense USING btree (descr);
CREATE INDEX cl_expense_end_date ON public.cl_expense USING btree (ends_on);
CREATE INDEX cl_expense_locked_id ON public.cl_expense USING btree (locked_id);
CREATE INDEX cl_expense_modified_id ON public.cl_expense USING btree (modified_id);
CREATE INDEX cl_expense_start_date ON public.cl_expense USING btree (starts_on);
CREATE INDEX cl_expense_state ON public.cl_expense USING btree (state);


-- public.cl_expense_payment definition

-- Drop table

-- DROP TABLE public.cl_expense_payment;

CREATE TABLE public.cl_expense_payment (
	payment_id serial4 NOT NULL,
	expense_id int4 NOT NULL,
	stamp timestamptz NULL,
	transaction_id int4 NULL,
	purse_id int4 NULL,
	CONSTRAINT cl_expense_payment_pkey PRIMARY KEY (payment_id),
	CONSTRAINT cl_expense_payment_expense_id_fkey FOREIGN KEY (expense_id) REFERENCES public.cl_expense(expense_id)
);
CREATE INDEX cl_expense_payment_expense_id ON public.cl_expense_payment USING btree (expense_id);
CREATE INDEX cl_expense_payment_stamp ON public.cl_expense_payment USING btree (stamp);


-- public.cl_salary definition

-- Drop table

-- DROP TABLE public.cl_salary;

CREATE TABLE public.cl_salary (
	company_id int4 NOT NULL,
	salary_id serial4 NOT NULL,
	office_id int4 NOT NULL,
	user_id int4 NOT NULL,
	"month" int4 DEFAULT 1 NOT NULL,
	"year" int4 DEFAULT 2000 NOT NULL,
	created timestamptz DEFAULT now() NOT NULL,
	modified timestamptz DEFAULT now() NOT NULL,
	"locked" bool DEFAULT false NOT NULL,
	created_id int4 NOT NULL,
	modified_id int4 NOT NULL,
	locked_id int4 NULL,
	lock_notify_ids jsonb DEFAULT '[]'::jsonb NOT NULL,
	payment_notify_ids jsonb DEFAULT '[]'::jsonb NOT NULL,
	payment_accept_notify_ids jsonb DEFAULT '[]'::jsonb NOT NULL,
	comment_notify_ids jsonb DEFAULT '[]'::jsonb NOT NULL,
	CONSTRAINT cl_salary_month_year_user_id_key UNIQUE (month, year, user_id),
	CONSTRAINT cl_salary_pkey PRIMARY KEY (salary_id),
	CONSTRAINT cl_salary_user_id FOREIGN KEY (user_id) REFERENCES public."user"(user_id) ON DELETE RESTRICT
);
CREATE INDEX cl_salary_company_id ON public.cl_salary USING btree (company_id);
CREATE INDEX cl_salary_created ON public.cl_salary USING btree (created);
CREATE INDEX cl_salary_created_id ON public.cl_salary USING btree (created_id);
CREATE INDEX cl_salary_locked_id ON public.cl_salary USING btree (locked_id);
CREATE INDEX cl_salary_modified ON public.cl_salary USING btree (modified);
CREATE INDEX cl_salary_modified_id ON public.cl_salary USING btree (modified_id);
CREATE INDEX cl_salary_office_id ON public.cl_salary USING btree (office_id);
CREATE INDEX cl_salary_user_id ON public.cl_salary USING btree (user_id);


-- public.cl_salary_attach definition

-- Drop table

-- DROP TABLE public.cl_salary_attach;

CREATE TABLE public.cl_salary_attach (
	attach_id serial4 NOT NULL,
	salary_id int4 NULL,
	worker_id int4 NULL,
	holiday_id int4 NULL,
	type_id int4 NULL,
	"source" varchar(100) NOT NULL,
	CONSTRAINT cl_salary_attach_pkey PRIMARY KEY (attach_id),
	CONSTRAINT cl_salary_attach_salary_id_fkey FOREIGN KEY (salary_id) REFERENCES public.cl_salary(salary_id) ON DELETE CASCADE
);
CREATE INDEX cl_salary_attach_holiday_id ON public.cl_salary_attach USING btree (holiday_id);
CREATE INDEX cl_salary_attach_salary_id ON public.cl_salary_attach USING btree (salary_id);
CREATE INDEX cl_salary_attach_type_id ON public.cl_salary_attach USING btree (type_id);
CREATE INDEX cl_salary_attach_worker_id ON public.cl_salary_attach USING btree (worker_id);


-- public.cl_salary_b_trip definition

-- Drop table

-- DROP TABLE public.cl_salary_b_trip;

CREATE TABLE public.cl_salary_b_trip (
	trip_id serial4 NOT NULL,
	tcreated timestamptz DEFAULT now() NOT NULL,
	tstart timestamptz NOT NULL,
	tend timestamptz NOT NULL,
	company_id int4 NOT NULL,
	office_id int4 NOT NULL,
	worker_id int4 NOT NULL,
	city_id int4 NOT NULL,
	city_ret_id int4 NULL,
	task varchar(120) NULL,
	car_id int4 NULL,
	settings jsonb DEFAULT '{}'::jsonb NOT NULL,
	CONSTRAINT cl_salary_b_trip_pkey PRIMARY KEY (trip_id),
	CONSTRAINT cl_salary_b_trip_car_id_fkey FOREIGN KEY (car_id) REFERENCES public.cl_salary_car(car_id) ON DELETE RESTRICT,
	CONSTRAINT cl_salary_b_trip_city_id FOREIGN KEY (city_id) REFERENCES public.city_details(city_id) ON DELETE RESTRICT,
	CONSTRAINT cl_salary_b_trip_city_ret_id FOREIGN KEY (city_ret_id) REFERENCES public.city_details(city_id) ON DELETE RESTRICT,
	CONSTRAINT cl_salary_b_trip_company_id FOREIGN KEY (company_id) REFERENCES public.company(company_id) ON DELETE RESTRICT,
	CONSTRAINT cl_salary_b_trip_office_id_fkey FOREIGN KEY (office_id) REFERENCES public.office(office_id) ON DELETE RESTRICT,
	CONSTRAINT cl_salary_b_trip_worker_id FOREIGN KEY (worker_id) REFERENCES public."user"(user_id) ON DELETE RESTRICT
);
CREATE INDEX cl_salary_b_trip_car_id_idx ON public.cl_salary_b_trip USING btree (car_id);
CREATE INDEX cl_salary_b_trip_city_id_idx ON public.cl_salary_b_trip USING btree (city_id);
CREATE INDEX cl_salary_b_trip_city_ret_id_idx ON public.cl_salary_b_trip USING btree (city_ret_id);
CREATE INDEX cl_salary_b_trip_company_id_idx ON public.cl_salary_b_trip USING btree (company_id);
CREATE INDEX cl_salary_b_trip_office_id_idx ON public.cl_salary_b_trip USING btree (office_id);
CREATE INDEX cl_salary_b_trip_task_idx ON public.cl_salary_b_trip USING btree (task);
CREATE INDEX cl_salary_b_trip_tcreated_idx ON public.cl_salary_b_trip USING btree (tcreated);
CREATE INDEX cl_salary_b_trip_tend_idx ON public.cl_salary_b_trip USING btree (tend);
CREATE INDEX cl_salary_b_trip_tstart_idx ON public.cl_salary_b_trip USING btree (tstart);
CREATE INDEX cl_salary_b_trip_worker_id_idx ON public.cl_salary_b_trip USING btree (worker_id);


-- public.cl_salary_b_trip_day definition

-- Drop table

-- DROP TABLE public.cl_salary_b_trip_day;

CREATE TABLE public.cl_salary_b_trip_day (
	day_id serial4 NOT NULL,
	trip_id int4 NOT NULL,
	arrived_at varchar(90) NULL,
	tarrived timestamptz NULL,
	tleave timestamptz NULL,
	free_food bool DEFAULT false NOT NULL,
	free_sleep bool DEFAULT false NOT NULL,
	signature_id int4 NOT NULL,
	CONSTRAINT cl_salary_b_trip_day_pkey PRIMARY KEY (day_id),
	CONSTRAINT cl_salary_b_trip_day_trip_id_fkey FOREIGN KEY (trip_id) REFERENCES public.cl_salary_b_trip(trip_id) ON DELETE CASCADE
);
CREATE INDEX cl_salary_b_trip_day_tarrived_idx ON public.cl_salary_b_trip_day USING btree (tarrived);
CREATE INDEX cl_salary_b_trip_day_trip_id_idx ON public.cl_salary_b_trip_day USING btree (trip_id);


-- public.cl_salary_comments definition

-- Drop table

-- DROP TABLE public.cl_salary_comments;

CREATE TABLE public.cl_salary_comments (
	comment_id serial4 NOT NULL,
	salary_id int4 NOT NULL,
	created timestamptz NOT NULL,
	creator_id int4 DEFAULT 1 NOT NULL,
	"comment" text NULL,
	CONSTRAINT cl_salary_comments_pkey PRIMARY KEY (comment_id),
	CONSTRAINT cl_salary_comments_salary_id_fkey FOREIGN KEY (salary_id) REFERENCES public.cl_salary(salary_id) ON DELETE RESTRICT
);
CREATE INDEX cl_salary_comments_salary_id ON public.cl_salary_comments USING btree (salary_id);


-- public.cl_salary_department definition

-- Drop table

-- DROP TABLE public.cl_salary_department;

CREATE TABLE public.cl_salary_department (
	dep_id serial4 NOT NULL,
	obj_id int4 NOT NULL,
	wh_id int4 NULL,
	"name" varchar(90) NULL,
	CONSTRAINT cl_salary_department_pkey PRIMARY KEY (dep_id),
	CONSTRAINT cl_salary_department_obj_id_fkey FOREIGN KEY (obj_id) REFERENCES public.cl_salary_object(obj_id) ON DELETE CASCADE
);
CREATE INDEX cl_salary_department_name_idx ON public.cl_salary_department USING btree (name);
CREATE INDEX cl_salary_department_obj_id_idx ON public.cl_salary_department USING btree (obj_id);
CREATE INDEX cl_salary_department_wh_id_idx ON public.cl_salary_department USING btree (wh_id);


-- public.cl_salary_line definition

-- Drop table

-- DROP TABLE public.cl_salary_line;

CREATE TABLE public.cl_salary_line (
	line_id serial4 NOT NULL,
	salary_id int4 NULL,
	worker_id int4 NULL,
	type_id int4 NOT NULL,
	sum float8 NULL,
	sum_currency int4 NULL,
	days int2 DEFAULT 0 NOT NULL,
	"position" int4 NULL,
	payment_method_id int2 DEFAULT 0 NOT NULL,
	note varchar(60) NULL,
	CONSTRAINT cl_salary_line_pkey PRIMARY KEY (line_id),
	CONSTRAINT cl_salary_line_salary_id_fkey FOREIGN KEY (salary_id) REFERENCES public.cl_salary(salary_id) ON DELETE RESTRICT,
	CONSTRAINT cl_salary_line_user_id FOREIGN KEY (worker_id) REFERENCES public."user"(user_id) ON DELETE RESTRICT
);
CREATE INDEX cl_salary_line_salary_id ON public.cl_salary_line USING btree (salary_id);
CREATE INDEX cl_salary_line_user_id ON public.cl_salary_line USING btree (worker_id);


-- public.cl_salary_overtime definition

-- Drop table

-- DROP TABLE public.cl_salary_overtime;

CREATE TABLE public.cl_salary_overtime (
	ot_id serial4 NOT NULL,
	user_ids jsonb DEFAULT '[]'::jsonb NOT NULL,
	tstart timestamptz NOT NULL,
	tend timestamptz NOT NULL,
	modifier_id int4 NOT NULL,
	note varchar(90) NULL,
	CONSTRAINT cl_salary_overtime_pkey PRIMARY KEY (ot_id),
	CONSTRAINT cl_salary_overtime_modifier FOREIGN KEY (modifier_id) REFERENCES public."user"(user_id) ON DELETE RESTRICT
);
CREATE INDEX cl_salary_overtime_modifier_id_idx ON public.cl_salary_overtime USING btree (modifier_id);
CREATE INDEX cl_salary_overtime_note_idx ON public.cl_salary_overtime USING btree (note);
CREATE INDEX cl_salary_overtime_tend_idx ON public.cl_salary_overtime USING btree (tend);
CREATE INDEX cl_salary_overtime_tstart_idx ON public.cl_salary_overtime USING btree (tstart);
CREATE INDEX cl_salary_overtime_user_ids_idx ON public.cl_salary_overtime USING btree (user_ids);


-- public.cl_salary_payment definition

-- Drop table

-- DROP TABLE public.cl_salary_payment;

CREATE TABLE public.cl_salary_payment (
	payment_id serial4 NOT NULL,
	salary_id int4 NOT NULL,
	purse_id int4 NULL,
	sum numeric(12, 4) NOT NULL,
	sum_currency int2 NOT NULL,
	"comment" text NULL,
	state int4 DEFAULT 0 NOT NULL,
	event_id jsonb DEFAULT '[]'::jsonb NOT NULL,
	created_id int4 NULL,
	accepted_id int4 NULL,
	paid_id int4 NULL,
	payment_method_id int4 DEFAULT 0 NOT NULL,
	"date" timestamptz DEFAULT now() NOT NULL,
	paid_date timestamptz NULL,
	CONSTRAINT cl_salary_payment_pkey PRIMARY KEY (payment_id),
	CONSTRAINT cl_salary_payment_salary_id_fkey FOREIGN KEY (salary_id) REFERENCES public.cl_salary(salary_id) ON DELETE CASCADE
);
CREATE INDEX cl_salary_payment_salary_id ON public.cl_salary_payment USING btree (salary_id);


-- public.cl_salary_work_contract definition

-- Drop table

-- DROP TABLE public.cl_salary_work_contract;

CREATE TABLE public.cl_salary_work_contract (
	contract_id serial4 NOT NULL,
	worker_id int4 NOT NULL,
	stamp timestamptz NULL,
	stamp_start timestamptz NULL,
	"document" int4 NULL,
	contract_law_type int4 NULL,
	nkpd_code int4 NULL,
	daily_work_time int4 NULL,
	test_period int4 NULL,
	salary_sum numeric(12, 2) NULL,
	salary_sum_currency int4 NULL,
	salary_period int4 NULL,
	payment_date int4 NULL,
	base_holiday_len int4 NULL,
	extra_holiday_len int4 NULL,
	worker_type_id int4 NULL,
	state int4 NULL,
	work_stage int4 NULL,
	work_stage_specialty int4 NULL,
	CONSTRAINT cl_salary_work_contract_pkey PRIMARY KEY (contract_id),
	CONSTRAINT cl_salary_work_contract_worker_id FOREIGN KEY (worker_id) REFERENCES public."user"(user_id) ON DELETE RESTRICT
);
CREATE INDEX cl_salary_work_contract_document ON public.cl_salary_work_contract USING btree (document);
CREATE INDEX cl_salary_work_contract_law_type ON public.cl_salary_work_contract USING btree (contract_law_type);
CREATE INDEX cl_salary_work_contract_worker_id ON public.cl_salary_work_contract USING btree (worker_id);


-- public.cl_salary_worker definition

-- Drop table

-- DROP TABLE public.cl_salary_worker;

CREATE TABLE public.cl_salary_worker (
	worker_id int4 NOT NULL,
	address_id int4 NULL,
	current_address_id int4 NULL,
	type_id int4 NOT NULL,
	stamp_start timestamptz NULL,
	stamp_end timestamptz NULL,
	egn varchar(10) NULL,
	lkn varchar(15) NULL,
	lk_pub_stamp timestamptz NULL,
	lk_pub_source varchar(50) NULL,
	education int4 NULL,
	specialty varchar(80) NULL,
	second_specialty varchar(80) NULL,
	work_stage int4 NULL,
	work_stage_specialty int4 NULL,
	holidays_left int4 DEFAULT 0 NOT NULL,
	holidays_change timestamptz DEFAULT now() NOT NULL,
	library_id int4 NULL,
	active bool DEFAULT true NOT NULL,
	obj_id int4 NOT NULL,
	dep_id int4 NOT NULL,
	wh_id int4 NULL,
	company_id int4 NOT NULL,
	iban jsonb NULL,
	mrates jsonb NULL,
	CONSTRAINT cl_salary_worker_pkey PRIMARY KEY (worker_id),
	CONSTRAINT cl_salary_worker_dep_id_fkey FOREIGN KEY (dep_id) REFERENCES public.cl_salary_department(dep_id) ON DELETE RESTRICT,
	CONSTRAINT cl_salary_worker_obj_id_fkey FOREIGN KEY (obj_id) REFERENCES public.cl_salary_object(obj_id) ON DELETE RESTRICT,
	CONSTRAINT cl_salary_worker_type_id_fkey FOREIGN KEY (type_id) REFERENCES public.cl_salary_worker_type(type_id) ON DELETE RESTRICT,
	CONSTRAINT cl_salary_worker_user_id FOREIGN KEY (worker_id) REFERENCES public."user"(user_id) ON DELETE RESTRICT,
	CONSTRAINT cl_salary_worker_worker_id FOREIGN KEY (worker_id) REFERENCES public."user"(user_id) ON DELETE RESTRICT
);
CREATE INDEX cl_salary_worker_active_idx ON public.cl_salary_worker USING btree (active);
CREATE INDEX cl_salary_worker_dep_id_idx ON public.cl_salary_worker USING btree (dep_id);
CREATE INDEX cl_salary_worker_egn_idx ON public.cl_salary_worker USING btree (egn);
CREATE INDEX cl_salary_worker_lkn_idx ON public.cl_salary_worker USING btree (lkn);
CREATE INDEX cl_salary_worker_obj_id_idx ON public.cl_salary_worker USING btree (obj_id);
CREATE INDEX cl_salary_worker_stamp_end_idx ON public.cl_salary_worker USING btree (stamp_end);
CREATE INDEX cl_salary_worker_stamp_start_idx ON public.cl_salary_worker USING btree (stamp_start);
CREATE INDEX cl_salary_worker_wh_id_idx ON public.cl_salary_worker USING btree (wh_id);


-- public.cl_salary_worker_comment definition

-- Drop table

-- DROP TABLE public.cl_salary_worker_comment;

CREATE TABLE public.cl_salary_worker_comment (
	comment_id serial4 NOT NULL,
	worker_id int4 NULL,
	created timestamptz NOT NULL,
	creator_id int4 DEFAULT 1 NOT NULL,
	"comment" text NULL,
	CONSTRAINT cl_salary_worker_comment_pkey PRIMARY KEY (comment_id),
	CONSTRAINT cl_salary_worker_comment_worker_id FOREIGN KEY (worker_id) REFERENCES public."user"(user_id) ON DELETE RESTRICT,
	CONSTRAINT cl_salary_worker_comment_worker_id_fkey FOREIGN KEY (worker_id) REFERENCES public.cl_salary_worker(worker_id) ON DELETE CASCADE
);
CREATE INDEX cl_salary_worker_comment_worker_id_idx ON public.cl_salary_worker_comment USING btree (worker_id);


-- public.cl_sl_comment definition

-- Drop table

-- DROP TABLE public.cl_sl_comment;

CREATE TABLE public.cl_sl_comment (
	comment_id serial4 NOT NULL,
	line_id int4 NOT NULL,
	created timestamptz DEFAULT now() NOT NULL,
	creator_id int4 NOT NULL,
	"comment" text NULL,
	addressed_id jsonb DEFAULT '[]'::jsonb NOT NULL,
	CONSTRAINT cl_sl_comment_pkey PRIMARY KEY (comment_id),
	CONSTRAINT cl_sl_comment_line_id_fkey FOREIGN KEY (line_id) REFERENCES public.cl_salary_line(line_id) ON DELETE CASCADE
);
CREATE INDEX cl_sl_comment_created_idx ON public.cl_sl_comment USING btree (created);
CREATE INDEX cl_sl_comment_line_id_idx ON public.cl_sl_comment USING btree (line_id);


-- public.company_bank_account definition

-- Drop table

-- DROP TABLE public.company_bank_account;

CREATE TABLE public.company_bank_account (
	bank_account_id serial4 NOT NULL,
	company_id int4 NULL,
	bank text NULL,
	bank_iban varchar(60) NULL,
	bank_bic varchar(60) NULL,
	customer_id int4 NULL,
	supplier_id int4 NULL,
	CONSTRAINT company_bank_account_company_id_fkey FOREIGN KEY (company_id) REFERENCES public.company(company_id) ON DELETE CASCADE,
	CONSTRAINT company_bank_account_customer_id_fkey FOREIGN KEY (customer_id) REFERENCES public.customer(customer_id) ON DELETE CASCADE,
	CONSTRAINT company_bank_account_supplier_id_fkey FOREIGN KEY (supplier_id) REFERENCES public.supplier(supplier_id) ON DELETE CASCADE
);
CREATE INDEX company_bank_account_company_id ON public.company_bank_account USING btree (company_id);
CREATE INDEX company_bank_account_customer_id ON public.company_bank_account USING btree (customer_id);
CREATE INDEX company_bank_account_supplier_id ON public.company_bank_account USING btree (supplier_id);


-- public.contr_contact definition

-- Drop table

-- DROP TABLE public.contr_contact;

CREATE TABLE public.contr_contact (
	contact_id serial4 NOT NULL,
	customer_id int4 NULL,
	supplier_id int4 NULL,
	first_name varchar(50) NULL,
	family_name varchar(50) NULL,
	phone varchar(80) NULL,
	contactdata text NULL,
	position_id int4 NULL,
	email varchar(50) NULL,
	email_extra varchar(50) NULL,
	use_for_sms bool DEFAULT false NOT NULL,
	CONSTRAINT contr_contact_pkey PRIMARY KEY (contact_id),
	CONSTRAINT contr_contact_customer_id_fkey FOREIGN KEY (customer_id) REFERENCES public.customer(customer_id) ON DELETE CASCADE,
	CONSTRAINT contr_contact_position_id_fkey FOREIGN KEY (position_id) REFERENCES public.contr_position(position_id),
	CONSTRAINT contr_contact_supplier_id_fkey FOREIGN KEY (supplier_id) REFERENCES public.supplier(supplier_id) ON DELETE CASCADE
);
CREATE INDEX contr_contact_customer ON public.contr_contact USING btree (customer_id);
CREATE INDEX contr_contact_positon ON public.contr_contact USING btree (position_id);
CREATE INDEX contr_contact_supplier ON public.contr_contact USING btree (supplier_id);


-- public.contr_cust_comments definition

-- Drop table

-- DROP TABLE public.contr_cust_comments;

CREATE TABLE public.contr_cust_comments (
	comment_id serial4 NOT NULL,
	customer_id int4 NULL,
	created timestamptz NOT NULL,
	creator_id int4 DEFAULT 1 NOT NULL,
	"comment" text NULL,
	supplier_id int4 NULL,
	company_id int4 NULL,
	CONSTRAINT contr_cust_comments_pkey PRIMARY KEY (comment_id),
	CONSTRAINT contr_cust_comments_company_id_fkey FOREIGN KEY (company_id) REFERENCES public.company(company_id) ON DELETE CASCADE,
	CONSTRAINT contr_cust_comments_customer_id_fkey FOREIGN KEY (customer_id) REFERENCES public.customer(customer_id),
	CONSTRAINT contr_cust_comments_supplier_id_fkey FOREIGN KEY (supplier_id) REFERENCES public.supplier(supplier_id) ON DELETE CASCADE
);
CREATE INDEX contr_cust_comments_company_id_idx ON public.contr_cust_comments USING btree (company_id);
CREATE INDEX contr_cust_comments_created_idx ON public.contr_cust_comments USING btree (created);
CREATE INDEX contr_cust_comments_customer_id ON public.contr_cust_comments USING btree (customer_id);
CREATE INDEX contr_cust_comments_supplier_id_idx ON public.contr_cust_comments USING btree (supplier_id);


-- public.contr_u_company definition

-- Drop table

-- DROP TABLE public.contr_u_company;

CREATE TABLE public.contr_u_company (
	user_id int4 NOT NULL,
	company_ids jsonb DEFAULT '[]'::jsonb NOT NULL,
	CONSTRAINT contr_u_company_pkey PRIMARY KEY (user_id),
	CONSTRAINT contr_u_company_user_id_fkey FOREIGN KEY (user_id) REFERENCES public."user"(user_id) ON DELETE RESTRICT
);
CREATE INDEX contr_u_company_company_ids_idx ON public.contr_u_company USING btree (company_ids);
CREATE INDEX contr_u_company_user_id_idx ON public.contr_u_company USING btree (user_id);


-- public.credit_purse definition

-- Drop table

-- DROP TABLE public.credit_purse;

CREATE TABLE public.credit_purse (
	purse_id serial4 NOT NULL,
	customer_id int4 NULL,
	user_id int4 NULL,
	modified_id int4 NULL,
	modified_time timestamptz DEFAULT now() NOT NULL,
	freeze_time timestamptz NULL,
	freeze_type int2 DEFAULT 0::smallint NOT NULL,
	credits numeric(12, 2) NULL,
	type_id int2 NOT NULL,
	CONSTRAINT credit_purse_pkey PRIMARY KEY (purse_id),
	CONSTRAINT credit_purse_cmodified_id FOREIGN KEY (modified_id) REFERENCES public."user"(user_id) ON DELETE RESTRICT,
	CONSTRAINT credit_purse_type_id_fkey FOREIGN KEY (type_id) REFERENCES public.credit_purse_type(type_id)
);
CREATE INDEX credit_purse_customer_id ON public.credit_purse USING btree (customer_id);
CREATE INDEX credit_purse_freeze_time ON public.credit_purse USING btree (freeze_time);
CREATE INDEX credit_purse_freeze_type ON public.credit_purse USING btree (freeze_type);
CREATE INDEX credit_purse_modified_id ON public.credit_purse USING btree (modified_id);
CREATE INDEX credit_purse_modified_time ON public.credit_purse USING btree (modified_time);
CREATE INDEX credit_purse_user_id ON public.credit_purse USING btree (user_id);
CREATE INDEX credits ON public.credit_purse USING btree (credits);


-- public.credit_transaction definition

-- Drop table

-- DROP TABLE public.credit_transaction;

CREATE TABLE public.credit_transaction (
	transaction_id serial4 NOT NULL,
	from_purse_id int4 NULL,
	to_purse_id int4 NULL,
	modified_id int4 NULL,
	modified_time timestamptz DEFAULT now() NOT NULL,
	"type" int2 DEFAULT 0 NOT NULL,
	credits numeric(12, 2) NULL,
	link varchar(90) NULL,
	CONSTRAINT credit_transaction_pkey PRIMARY KEY (transaction_id),
	CONSTRAINT credit_transaction_from_purse_id_fkey FOREIGN KEY (from_purse_id) REFERENCES public.credit_purse(purse_id) ON DELETE RESTRICT,
	CONSTRAINT credit_transaction_to_purse_id_fkey FOREIGN KEY (to_purse_id) REFERENCES public.credit_purse(purse_id) ON DELETE RESTRICT
);
CREATE INDEX credit_transaction_from_purse_id ON public.credit_transaction USING btree (from_purse_id);
CREATE INDEX credit_transaction_modified_id ON public.credit_transaction USING btree (modified_id);
CREATE INDEX credit_transaction_modified_time ON public.credit_transaction USING btree (modified_time);
CREATE INDEX credit_transaction_to_purse_id ON public.credit_transaction USING btree (to_purse_id);
CREATE INDEX credit_transaction_type ON public.credit_transaction USING btree (type);


-- public.invoice_note definition

-- Drop table

-- DROP TABLE public.invoice_note;

CREATE TABLE public.invoice_note (
	invoice_note_id serial4 NOT NULL,
	"document" int4 NOT NULL,
	customer_id int4 NOT NULL,
	company_id int4 NOT NULL,
	payment_id int4 NOT NULL,
	user_id int4 NOT NULL,
	"date" timestamptz DEFAULT now() NOT NULL,
	CONSTRAINT invoice_note_company_id_document_key UNIQUE (company_id, document),
	CONSTRAINT invoice_note_pkey PRIMARY KEY (invoice_note_id),
	CONSTRAINT invoice_note_company_id_fkey FOREIGN KEY (company_id) REFERENCES public.company(company_id) ON DELETE RESTRICT,
	CONSTRAINT invoice_note_customer_id_fkey FOREIGN KEY (customer_id) REFERENCES public.customer(customer_id) ON DELETE RESTRICT,
	CONSTRAINT invoice_note_user_id FOREIGN KEY (user_id) REFERENCES public."user"(user_id) ON DELETE RESTRICT
);
CREATE INDEX invoice_note_company_id ON public.invoice_note USING btree (company_id);
CREATE INDEX invoice_note_customer_id ON public.invoice_note USING btree (customer_id);


-- public.personnel_worker_day definition

-- Drop table

-- DROP TABLE public.personnel_worker_day;

CREATE TABLE public.personnel_worker_day (
	worker_day_id serial4 NOT NULL,
	worker_id int4 NOT NULL,
	work_minutes int4 DEFAULT 0 NOT NULL,
	break_minutes int4 DEFAULT 0 NOT NULL,
	deffer_minutes int4 DEFAULT 0 NOT NULL,
	first_enter int4 DEFAULT 0 NOT NULL,
	last_leave int4 DEFAULT 0 NOT NULL,
	last_action timestamptz NULL,
	last_zone int4 DEFAULT 0 NOT NULL,
	finished bool DEFAULT false NOT NULL,
	CONSTRAINT personnel_worker_day_pkey PRIMARY KEY (worker_day_id),
	CONSTRAINT personnel_worker_day_worker_id_fkey FOREIGN KEY (worker_id) REFERENCES public.cl_salary_worker(worker_id) ON DELETE CASCADE
);
CREATE INDEX personnel_worker_day_worker ON public.personnel_worker_day USING btree (worker_id);
CREATE INDEX personnel_worker_modify_date ON public.personnel_worker_day USING btree (last_action);


-- public.product_default definition

-- Drop table

-- DROP TABLE public.product_default;

CREATE TABLE public.product_default (
	product_default_id serial4 NOT NULL,
	product_type_id int2 NOT NULL,
	product_code varchar(100) NULL,
	"ref" int4 NULL,
	title varchar(200) NOT NULL,
	price1 numeric(12, 4) DEFAULT 0 NOT NULL,
	price1_currency_id int2 NOT NULL,
	tax1_product_id int2 NULL,
	price2 numeric(12, 4) DEFAULT 0 NOT NULL,
	price2_currency_id int2 NOT NULL,
	tax2_product_id int2 NULL,
	is_variational bool DEFAULT false NOT NULL,
	active bool DEFAULT false NOT NULL,
	date_created timestamptz DEFAULT now() NOT NULL,
	date_modified timestamptz DEFAULT now() NOT NULL,
	"data" _int4 NULL,
	tsv tsvector NULL,
	part_number varchar(120) NULL,
	man_id int4 NULL,
	ext_numbers text NULL,
	wtitle text NULL,
	CONSTRAINT product_default_pkey PRIMARY KEY (product_default_id),
	CONSTRAINT product_default_man_id_fkey FOREIGN KEY (man_id) REFERENCES public.product_manufacturer(man_id) ON DELETE SET NULL,
	CONSTRAINT product_default_price1_currency_id_fkey FOREIGN KEY (price1_currency_id) REFERENCES public.currency(currency_id) ON DELETE RESTRICT,
	CONSTRAINT product_default_price2_currency_id_fkey FOREIGN KEY (price2_currency_id) REFERENCES public.currency(currency_id) ON DELETE RESTRICT,
	CONSTRAINT product_default_product_type_id_fkey FOREIGN KEY (product_type_id) REFERENCES public.product_type(product_type_id) ON DELETE RESTRICT
);
CREATE INDEX product_default_data ON public.product_default USING gin (data);
CREATE INDEX product_default_man_id ON public.product_default USING btree (man_id);
CREATE INDEX product_default_part_number ON public.product_default USING btree (part_number);
CREATE INDEX product_default_title ON public.product_default USING btree (title);
CREATE INDEX product_default_tsv ON public.product_default USING gin (tsv);
CREATE INDEX product_default_wtitle ON public.product_default USING btree (wtitle);
CREATE INDEX product_default_wtitle_idx ON public.product_default USING gin (wtitle gin_trgm_ops);

-- Table Triggers

create trigger product_tsv_vector before
insert
    or
update
    on
    public.product_default for each row execute function product_tsv_vector();


-- public.product_discount definition

-- Drop table

-- DROP TABLE public.product_discount;

CREATE TABLE public.product_discount (
	product_discount_id serial4 NOT NULL,
	product_default_id int4 NOT NULL,
	active bool DEFAULT false NOT NULL,
	priority int2 DEFAULT 0 NOT NULL,
	quantity int2 DEFAULT 0 NOT NULL,
	price_modification numeric(12, 4) DEFAULT 0 NOT NULL,
	price_modification_type int2 DEFAULT 0 NOT NULL,
	date_start timestamptz NULL,
	date_end timestamptz NULL,
	CONSTRAINT product_discount_pkey PRIMARY KEY (product_discount_id),
	CONSTRAINT product_discount_product_default_id_fkey FOREIGN KEY (product_default_id) REFERENCES public.product_default(product_default_id) ON DELETE CASCADE
);
CREATE INDEX product_discount_product_default_id ON public.product_discount USING btree (product_default_id);


-- public.product_group_map definition

-- Drop table

-- DROP TABLE public.product_group_map;

CREATE TABLE public.product_group_map (
	product_group_map_id serial4 NOT NULL,
	product_group_id int4 NOT NULL,
	product_default_id int4 NOT NULL,
	"position" int4 NULL,
	CONSTRAINT product_group_map_pkey PRIMARY KEY (product_group_map_id),
	CONSTRAINT product_group_map_product_group_id_product_default_id_key UNIQUE (product_group_id, product_default_id),
	CONSTRAINT product_group_map_product_default_id_fkey FOREIGN KEY (product_default_id) REFERENCES public.product_default(product_default_id) ON DELETE CASCADE,
	CONSTRAINT product_group_map_product_group_id_fkey FOREIGN KEY (product_group_id) REFERENCES public.product_group(product_group_id) ON DELETE CASCADE
);
CREATE INDEX product_group_map_product_default_id ON public.product_group_map USING btree (product_default_id);
CREATE INDEX product_group_map_product_group_id ON public.product_group_map USING btree (product_group_id);

-- Table Triggers

create trigger product_group_map_delete before
delete
    on
    public.product_group_map for each row execute function product_group_map_delete();
create trigger product_group_map_insert after
insert
    on
    public.product_group_map for each row execute function product_group_map_insert();
create trigger product_group_map_update after
update
    on
    public.product_group_map for each row execute function product_group_map_update();


-- public.product_multimedia definition

-- Drop table

-- DROP TABLE public.product_multimedia;

CREATE TABLE public.product_multimedia (
	product_multimedia_id serial4 NOT NULL,
	product_default_id int4 NOT NULL,
	product_attribute_data_id int4 NULL,
	"type" int2 DEFAULT 0 NOT NULL,
	main int2 DEFAULT 0 NOT NULL,
	"source" varchar(100) NOT NULL,
	alt varchar(100) NULL,
	CONSTRAINT product_multimedia_pkey PRIMARY KEY (product_multimedia_id),
	CONSTRAINT product_multimedia_product_default_id_fkey FOREIGN KEY (product_default_id) REFERENCES public.product_default(product_default_id) ON DELETE CASCADE
);
CREATE INDEX product_multimedia_product_attribute_data_id ON public.product_multimedia USING btree (product_attribute_data_id);
CREATE INDEX product_multimedia_product_default_id ON public.product_multimedia USING btree (product_default_id);


-- public.product_option_set definition

-- Drop table

-- DROP TABLE public.product_option_set;

CREATE TABLE public.product_option_set (
	product_default_id int4 NOT NULL,
	product_attribute_data_id int4 NOT NULL,
	price_modification numeric(12, 4) DEFAULT 0 NOT NULL,
	price_modification_type int2 DEFAULT 0 NOT NULL,
	weight_modification int2 DEFAULT 0 NOT NULL,
	CONSTRAINT product_option_set_pkey PRIMARY KEY (product_default_id, product_attribute_data_id),
	CONSTRAINT product_option_set_product_attribute_data_id_fkey FOREIGN KEY (product_attribute_data_id) REFERENCES public.product_attribute_data(product_attribute_data_id) ON DELETE CASCADE,
	CONSTRAINT product_option_set_product_default_id_fkey FOREIGN KEY (product_default_id) REFERENCES public.product_default(product_default_id) ON DELETE CASCADE
);


-- public.product_price definition

-- Drop table

-- DROP TABLE public.product_price;

CREATE TABLE public.product_price (
	product_price_id serial4 NOT NULL,
	product_default_id int4 NOT NULL,
	customer_group_id int4 NOT NULL,
	price numeric(12, 4) NOT NULL,
	CONSTRAINT product_price_pkey PRIMARY KEY (product_price_id),
	CONSTRAINT product_price_product_default_id_fkey FOREIGN KEY (product_default_id) REFERENCES public.product_default(product_default_id) ON DELETE CASCADE
);


-- public.product_settings definition

-- Drop table

-- DROP TABLE public.product_settings;

CREATE TABLE public.product_settings (
	product_default_id int4 NOT NULL,
	serial_type int2 NULL,
	quantity_unit_id int2 NOT NULL,
	quantity_unit_id2 int2 NULL,
	quantity_unit_factor int4 NULL,
	quantity_min numeric(12, 2) NULL,
	weight numeric(8, 4) NULL,
	weight_unit_id int2 NULL,
	"size" varchar(100) NULL,
	do_subtract bool DEFAULT true NOT NULL,
	sell_unavailable bool DEFAULT false NOT NULL,
	deactivate_unavailable bool DEFAULT false NOT NULL,
	activate_available bool DEFAULT false NOT NULL,
	supplier_id int4 NULL,
	"comment" text NULL,
	supplier_ids jsonb DEFAULT '[]'::jsonb NOT NULL,
	CONSTRAINT product_settings_pkey PRIMARY KEY (product_default_id),
	CONSTRAINT product_settings_product_default_id_fkey FOREIGN KEY (product_default_id) REFERENCES public.product_default(product_default_id) ON DELETE CASCADE,
	CONSTRAINT product_settings_quantity_unit_id2_fkey FOREIGN KEY (quantity_unit_id) REFERENCES public.quantity_unit(quantity_unit_id) ON DELETE RESTRICT,
	CONSTRAINT product_settings_quantity_unit_id_fkey FOREIGN KEY (quantity_unit_id) REFERENCES public.quantity_unit(quantity_unit_id) ON DELETE RESTRICT,
	CONSTRAINT product_settings_weight_unit_id_fkey FOREIGN KEY (weight_unit_id) REFERENCES public.weight_unit(weight_unit_id) ON DELETE RESTRICT
);


-- public.product_special definition

-- Drop table

-- DROP TABLE public.product_special;

CREATE TABLE public.product_special (
	product_special_id serial4 NOT NULL,
	product_default_id int4 NOT NULL,
	active bool DEFAULT false NOT NULL,
	priority int2 DEFAULT 0 NOT NULL,
	price_modification numeric(12, 4) DEFAULT 0 NOT NULL,
	price_modification_type int2 DEFAULT 0 NOT NULL,
	date_start timestamptz NULL,
	date_end timestamptz NULL,
	CONSTRAINT product_special_pkey PRIMARY KEY (product_special_id),
	CONSTRAINT product_special_product_default_id_fkey FOREIGN KEY (product_default_id) REFERENCES public.product_default(product_default_id) ON DELETE CASCADE
);
CREATE INDEX product_special_product_default_id ON public.product_special USING btree (product_default_id);


-- public.product_stats definition

-- Drop table

-- DROP TABLE public.product_stats;

CREATE TABLE public.product_stats (
	product_default_id int4 NOT NULL,
	viewed int4 DEFAULT 0 NOT NULL,
	bought int4 DEFAULT 0 NOT NULL,
	CONSTRAINT product_stats_pkey PRIMARY KEY (product_default_id),
	CONSTRAINT product_stats_product_default_id_fkey FOREIGN KEY (product_default_id) REFERENCES public.product_default(product_default_id) ON DELETE CASCADE
);


-- public.product_supply_code definition

-- Drop table

-- DROP TABLE public.product_supply_code;

CREATE TABLE public.product_supply_code (
	code_id serial4 NOT NULL,
	product_default_id int4 NOT NULL,
	supplier_id int4 NOT NULL,
	code varchar(90) NULL,
	CONSTRAINT product_supply_code_pkey PRIMARY KEY (code_id),
	CONSTRAINT product_supply_code_product_default_id_supplier_id_key UNIQUE (product_default_id, supplier_id),
	CONSTRAINT product_supply_code_product_default_id_fkey FOREIGN KEY (product_default_id) REFERENCES public.product_default(product_default_id) ON DELETE CASCADE
);
CREATE INDEX product_supply_code_code_idx ON public.product_supply_code USING btree (code);
CREATE INDEX product_supply_code_product_default_id_idx ON public.product_supply_code USING btree (product_default_id);
CREATE INDEX product_supply_code_supplier_id_idx ON public.product_supply_code USING btree (supplier_id);


-- public.product_value_set definition

-- Drop table

-- DROP TABLE public.product_value_set;

CREATE TABLE public.product_value_set (
	product_default_id int4 NOT NULL,
	product_attribute_id int4 NOT NULL,
	language_id int2 NOT NULL,
	attribute_value text NULL,
	CONSTRAINT product_value_set_pkey PRIMARY KEY (product_default_id, product_attribute_id, language_id),
	CONSTRAINT product_value_set_language_id_fkey FOREIGN KEY (language_id) REFERENCES public."language"(language_id) ON DELETE RESTRICT,
	CONSTRAINT product_value_set_product_attribute_id_fkey FOREIGN KEY (product_attribute_id) REFERENCES public.product_attribute(product_attribute_id) ON DELETE CASCADE,
	CONSTRAINT product_value_set_product_default_id_fkey FOREIGN KEY (product_default_id) REFERENCES public.product_default(product_default_id) ON DELETE CASCADE
);


-- public.product_variant_set definition

-- Drop table

-- DROP TABLE public.product_variant_set;

CREATE TABLE public.product_variant_set (
	product_default_id int4 NOT NULL,
	product_attribute_data_id int4 NOT NULL,
	price1_modification numeric(12, 4) DEFAULT 0 NOT NULL,
	price1_modification_type int2 DEFAULT 0 NOT NULL,
	price2_modification numeric(12, 4) DEFAULT 0 NOT NULL,
	price2_modification_type int2 DEFAULT 0 NOT NULL,
	weight_modification int2 DEFAULT 0 NOT NULL,
	attribute_data json NULL,
	CONSTRAINT product_variant_set_pkey PRIMARY KEY (product_default_id, product_attribute_data_id),
	CONSTRAINT product_variant_set_product_attribute_data_id_fkey FOREIGN KEY (product_attribute_data_id) REFERENCES public.product_attribute_data(product_attribute_data_id) ON DELETE CASCADE,
	CONSTRAINT product_variant_set_product_default_id_fkey FOREIGN KEY (product_default_id) REFERENCES public.product_default(product_default_id) ON DELETE CASCADE
);


-- public.product_web definition

-- Drop table

-- DROP TABLE public.product_web;

CREATE TABLE public.product_web (
	product_default_id int4 NOT NULL,
	language_id int2 NOT NULL,
	product_title varchar(200) NULL,
	web_url varchar(200) NULL,
	web_title varchar(200) NULL,
	web_keywords varchar(255) NULL,
	web_description varchar(255) NULL,
	content_short text NULL,
	"content" text NULL,
	tsv tsvector NULL,
	CONSTRAINT product_web_pkey PRIMARY KEY (product_default_id, language_id),
	CONSTRAINT product_web_language_id_fkey FOREIGN KEY (language_id) REFERENCES public."language"(language_id) ON DELETE RESTRICT,
	CONSTRAINT product_web_product_default_id_fkey FOREIGN KEY (product_default_id) REFERENCES public.product_default(product_default_id) ON DELETE CASCADE
);
CREATE INDEX product_web_tsv ON public.product_web USING gin (tsv);

-- Table Triggers

create trigger product_web_tsv_vector before
insert
    or
update
    on
    public.product_web for each row execute function product_web_tsv_vector();


-- public.purchase definition

-- Drop table

-- DROP TABLE public.purchase;

CREATE TABLE public.purchase (
	purchase_id serial4 NOT NULL,
	supplier_id int4 NULL,
	warehouse_id int4 NOT NULL,
	user_id int4 NULL,
	user_id_last int4 NULL,
	document_id int4 NULL,
	parent_id int4 NULL,
	"source" varchar(100) DEFAULT ''::character varying NULL,
	"comment" text DEFAULT ''::text NOT NULL,
	state int2 DEFAULT 0 NOT NULL,
	discount numeric(12, 2) NULL,
	discount_type int2 NULL,
	"ref" int4 NULL,
	"date" timestamptz DEFAULT now() NOT NULL,
	"locked" bool DEFAULT false NOT NULL,
	locked_id int4 NULL,
	sale_id int4 NULL,
	customer_id int4 NULL,
	date_modified timestamptz DEFAULT now() NOT NULL,
	lock_notify_ids jsonb DEFAULT '[]'::jsonb NOT NULL,
	change_notify_ids jsonb DEFAULT '[]'::jsonb NOT NULL,
	payment_notify_ids jsonb DEFAULT '[]'::jsonb NOT NULL,
	payment_accept_notify_ids jsonb DEFAULT '[]'::jsonb NOT NULL,
	comment_notify_ids jsonb DEFAULT '[]'::jsonb NOT NULL,
	involved jsonb DEFAULT '[]'::jsonb NOT NULL,
	is_paid int4 NULL,
	status_id int4 NULL,
	reason_id int4 NULL,
	library_id int4 NULL,
	company_id int4 NULL,
	office_id int4 NULL,
	type_id int4 NULL,
	delivered int4 DEFAULT 0 NOT NULL,
	authorize int2 DEFAULT 0 NOT NULL,
	auth_id int4 NULL,
	div_id int4 NULL,
	CONSTRAINT purchase_document_id_state_key UNIQUE (document_id, state),
	CONSTRAINT purchase_pkey PRIMARY KEY (purchase_id),
	CONSTRAINT purchase_auth_id FOREIGN KEY (auth_id) REFERENCES public."user"(user_id) ON DELETE RESTRICT,
	CONSTRAINT purchase_company_id FOREIGN KEY (company_id) REFERENCES public.company(company_id) ON DELETE RESTRICT,
	CONSTRAINT purchase_locked_id FOREIGN KEY (locked_id) REFERENCES public."user"(user_id) ON DELETE RESTRICT,
	CONSTRAINT purchase_office_id FOREIGN KEY (office_id) REFERENCES public.office(office_id) ON DELETE RESTRICT,
	CONSTRAINT purchase_reason_id_fkey FOREIGN KEY (reason_id) REFERENCES public.purchase_status_reason(reason_id),
	CONSTRAINT purchase_status_id_fkey FOREIGN KEY (status_id) REFERENCES public.purchase_status(status_id),
	CONSTRAINT purchase_supplier_id_fkey FOREIGN KEY (supplier_id) REFERENCES public.supplier(supplier_id) ON DELETE RESTRICT,
	CONSTRAINT purchase_type_id_fkey FOREIGN KEY (type_id) REFERENCES public.purchase_type(type_id) ON DELETE RESTRICT,
	CONSTRAINT purchase_user_id FOREIGN KEY (user_id) REFERENCES public."user"(user_id) ON DELETE RESTRICT,
	CONSTRAINT purchase_user_id_last FOREIGN KEY (user_id_last) REFERENCES public."user"(user_id) ON DELETE RESTRICT,
	CONSTRAINT purchase_warehouse_id_fkey FOREIGN KEY (warehouse_id) REFERENCES public.warehouse(warehouse_id) ON DELETE RESTRICT
);
CREATE INDEX purchase_auth_id_idx ON public.purchase USING btree (auth_id);
CREATE INDEX purchase_authorize_idx ON public.purchase USING btree (authorize);
CREATE INDEX purchase_company_id_idx ON public.purchase USING btree (company_id);
CREATE INDEX purchase_customer_id ON public.purchase USING btree (customer_id);
CREATE INDEX purchase_delivered_idx ON public.purchase USING btree (delivered);
CREATE INDEX purchase_div_id_idx ON public.purchase USING btree (div_id);
CREATE INDEX purchase_document_id_idx ON public.purchase USING btree (document_id);
CREATE INDEX purchase_involved_idx ON public.purchase USING btree (involved);
CREATE INDEX purchase_office_id_idx ON public.purchase USING btree (office_id);
CREATE INDEX purchase_parent_id_idx ON public.purchase USING btree (parent_id);
CREATE INDEX purchase_sale_id ON public.purchase USING btree (sale_id);
CREATE INDEX purchase_supplier_id ON public.purchase USING btree (supplier_id);
CREATE INDEX purchase_type_id_idx ON public.purchase USING btree (type_id);
CREATE INDEX purchase_warehouse_id ON public.purchase USING btree (warehouse_id);


-- public.purchase_attach definition

-- Drop table

-- DROP TABLE public.purchase_attach;

CREATE TABLE public.purchase_attach (
	purchase_attach_id serial4 NOT NULL,
	purchase_id int4 NOT NULL,
	"select" int4 NULL,
	"source" varchar(100) NOT NULL,
	"comment" text NULL,
	creator_id int4 NULL,
	"date" timestamptz NULL,
	CONSTRAINT purchase_attach_pkey PRIMARY KEY (purchase_attach_id),
	CONSTRAINT purchase_attach_purchase_id_fkey FOREIGN KEY (purchase_id) REFERENCES public.purchase(purchase_id) ON DELETE CASCADE
);
CREATE INDEX purchase_attach_purchase_id ON public.purchase_attach USING btree (purchase_id);
CREATE INDEX purchase_attach_source_idx ON public.purchase_attach USING btree (source);


-- public.purchase_data definition

-- Drop table

-- DROP TABLE public.purchase_data;

CREATE TABLE public.purchase_data (
	purchase_data_id serial4 NOT NULL,
	purchase_id int4 NOT NULL,
	product_id int4 NOT NULL,
	quantity numeric(12, 4) DEFAULT 0 NOT NULL,
	price numeric(12, 4) DEFAULT 0 NOT NULL,
	price_currency_id int2 NOT NULL,
	tax_product_id int2 NULL,
	"position" int4 DEFAULT 0 NOT NULL,
	serial_ids jsonb DEFAULT '[]'::jsonb NOT NULL,
	CONSTRAINT purchase_data_pkey PRIMARY KEY (purchase_data_id),
	CONSTRAINT purchase_data_purchase_id_fkey FOREIGN KEY (purchase_id) REFERENCES public.purchase(purchase_id) ON DELETE CASCADE
);
CREATE INDEX purchase_data_purchase_id ON public.purchase_data USING btree (purchase_id);


-- public.purchase_payment definition

-- Drop table

-- DROP TABLE public.purchase_payment;

CREATE TABLE public.purchase_payment (
	purchase_payment_id serial4 NOT NULL,
	purchase_id int4 NOT NULL,
	amount numeric(12, 4) NOT NULL,
	amount_currency_id int2 NOT NULL,
	payment_id int2 NOT NULL,
	"comment" text NULL,
	state int4 DEFAULT 0 NOT NULL,
	event_id jsonb DEFAULT '[]'::jsonb NOT NULL,
	created_id int4 NULL,
	accepted_id int4 NULL,
	paid_id int4 NULL,
	purse_id int4 NULL,
	"date" timestamptz DEFAULT now() NOT NULL,
	paid_date timestamptz NULL,
	CONSTRAINT purchase_payment_pkey PRIMARY KEY (purchase_payment_id),
	CONSTRAINT purchase_payment_purchase_id_fkey FOREIGN KEY (purchase_id) REFERENCES public.purchase(purchase_id) ON DELETE CASCADE
);
CREATE INDEX purchase_payment_purchase_id ON public.purchase_payment USING btree (purchase_id);


-- public.sale definition

-- Drop table

-- DROP TABLE public.sale;

CREATE TABLE public.sale (
	sale_id serial4 NOT NULL,
	sale_type_id int4 NULL,
	customer_id int4 NULL,
	company_id int4 NULL,
	phone varchar(60) NULL,
	country_id int4 NULL,
	country_zone_id int4 NULL,
	city text NULL,
	post varchar(30) NULL,
	address text NULL,
	warehouse_id int4 NULL,
	user_id int2 NULL,
	user_id_last int2 NULL,
	source_id int2 NULL,
	"comment" text NULL,
	user_comment text NULL,
	state int2 DEFAULT 0 NOT NULL,
	discount numeric(12, 2) NULL,
	discount_type int2 NULL,
	carrier_ship_id int4 NULL,
	sale_status_id int2 NULL,
	reason_id int4 NULL,
	"ref" int4 NULL,
	"date" timestamptz DEFAULT now() NOT NULL,
	date_modified timestamptz DEFAULT now() NOT NULL,
	"locked" bool DEFAULT false NOT NULL,
	locked_id int4 NULL,
	"object" varchar(70) NULL,
	lock_notify_ids jsonb DEFAULT '[]'::jsonb NOT NULL,
	change_notify_ids jsonb DEFAULT '[]'::jsonb NOT NULL,
	is_paid int4 DEFAULT 0 NOT NULL,
	comment_notify_ids jsonb DEFAULT '[]'::jsonb NOT NULL,
	trader_ids jsonb DEFAULT '[]'::jsonb NOT NULL,
	involved jsonb DEFAULT '[]'::jsonb NOT NULL,
	contact_id int4 NULL,
	office_id int4 NULL,
	completed bool DEFAULT false NOT NULL,
	document_id int4 NULL,
	parent_sale_id int4 NULL,
	library_id int4 NULL,
	valid_till timestamptz NULL,
	div_id int4 NULL,
	amount numeric(12, 3) DEFAULT 0 NOT NULL,
	amount_paid numeric(12, 3) DEFAULT 0 NOT NULL,
	amount_currency_id int2 DEFAULT 1 NOT NULL,
	damount numeric(12, 3) DEFAULT 0 NOT NULL,
	damount_currency_id int2 DEFAULT 1 NOT NULL,
	CONSTRAINT sale_pkey PRIMARY KEY (sale_id),
	CONSTRAINT sale_state_document_id_key UNIQUE (state, document_id),
	CONSTRAINT sale_country_id_fkey FOREIGN KEY (country_id) REFERENCES public.country(country_id) ON DELETE RESTRICT,
	CONSTRAINT sale_country_zone_id_fkey FOREIGN KEY (country_zone_id) REFERENCES public.country_zone(country_zone_id) ON DELETE RESTRICT,
	CONSTRAINT sale_customer_id_fkey FOREIGN KEY (customer_id) REFERENCES public.customer(customer_id) ON DELETE RESTRICT,
	CONSTRAINT sale_locked_id FOREIGN KEY (locked_id) REFERENCES public."user"(user_id) ON DELETE RESTRICT,
	CONSTRAINT sale_sale_status_id_fkey FOREIGN KEY (sale_status_id) REFERENCES public.sale_status(sale_status_id) ON DELETE RESTRICT,
	CONSTRAINT sale_source_id_fkey FOREIGN KEY (source_id) REFERENCES public.sale_source(source_id) ON DELETE CASCADE,
	CONSTRAINT sale_user_id FOREIGN KEY (user_id) REFERENCES public."user"(user_id) ON DELETE RESTRICT,
	CONSTRAINT sale_user_id_fkey FOREIGN KEY (user_id) REFERENCES public."user"(user_id) ON DELETE RESTRICT,
	CONSTRAINT sale_user_id_last FOREIGN KEY (user_id_last) REFERENCES public."user"(user_id) ON DELETE RESTRICT,
	CONSTRAINT sale_user_id_last_fkey FOREIGN KEY (user_id_last) REFERENCES public."user"(user_id) ON DELETE RESTRICT,
	CONSTRAINT sale_warehouse_id_fkey FOREIGN KEY (warehouse_id) REFERENCES public.warehouse(warehouse_id) ON DELETE RESTRICT
);
CREATE INDEX sale_amount_idx ON public.sale USING btree (amount);
CREATE INDEX sale_amount_paid_idx ON public.sale USING btree (amount_paid);
CREATE INDEX sale_country_id ON public.sale USING btree (country_id);
CREATE INDEX sale_country_zone_id ON public.sale USING btree (country_zone_id);
CREATE INDEX sale_customer_id ON public.sale USING btree (customer_id);
CREATE INDEX sale_div_id_idx ON public.sale USING btree (div_id);
CREATE INDEX sale_involved ON public.sale USING btree (involved);
CREATE INDEX sale_parent ON public.sale USING btree (parent_sale_id);
CREATE INDEX sale_reason ON public.sale USING btree (reason_id);
CREATE INDEX sale_sale_status_id ON public.sale USING btree (sale_status_id);
CREATE INDEX sale_source_id ON public.sale USING btree (source_id);
CREATE INDEX sale_type_sale_id ON public.sale USING btree (sale_type_id);
CREATE INDEX sale_valid_till_idx ON public.sale USING btree (valid_till);
CREATE INDEX sale_warehouse_id ON public.sale USING btree (warehouse_id);


-- public.sale_data definition

-- Drop table

-- DROP TABLE public.sale_data;

CREATE TABLE public.sale_data (
	sale_data_id serial4 NOT NULL,
	sale_id int4 NOT NULL,
	product_id int4 NULL,
	quantity numeric(12, 4) DEFAULT 0 NOT NULL,
	price numeric(12, 4) DEFAULT 0 NOT NULL,
	price_currency_id int2 NOT NULL,
	tax_product_id int2 NULL,
	serial_id int4 NULL,
	"name" text NULL,
	"position" int4 DEFAULT 0 NOT NULL,
	quantity_unit_id int2 DEFAULT 0 NOT NULL,
	CONSTRAINT sale_data_pkey PRIMARY KEY (sale_data_id),
	CONSTRAINT sale_data_sale_id_fkey FOREIGN KEY (sale_id) REFERENCES public.sale(sale_id) ON DELETE CASCADE
);
CREATE INDEX sale_data_position_idx ON public.sale_data USING btree ("position");
CREATE INDEX sale_data_sale_id ON public.sale_data USING btree (sale_id);


-- public.sale_data_comment definition

-- Drop table

-- DROP TABLE public.sale_data_comment;

CREATE TABLE public.sale_data_comment (
	sd_id int4 NOT NULL,
	"comment" text NULL,
	CONSTRAINT sale_data_comment_pkey PRIMARY KEY (sd_id),
	CONSTRAINT sale_data_comment_sd_id_fkey FOREIGN KEY (sd_id) REFERENCES public.sale_data(sale_data_id) ON DELETE CASCADE
);
CREATE INDEX sale_data_comment_sd_id_idx ON public.sale_data_comment USING btree (sd_id);


-- public.sale_hand_over definition

-- Drop table

-- DROP TABLE public.sale_hand_over;

CREATE TABLE public.sale_hand_over (
	ho_id serial4 NOT NULL,
	customer_id int4 NOT NULL,
	company_id int4 NOT NULL,
	warehouse_id int4 NOT NULL,
	user_id int4 NOT NULL,
	user_hand_id int4 NULL,
	user_ret_id int4 NULL,
	"comment" text NULL,
	created timestamptz NULL,
	handed timestamptz NULL,
	returned timestamptz NULL,
	amount numeric(12, 2) DEFAULT 0 NOT NULL,
	price_currency_id int4 NOT NULL,
	purse_id int4 NULL,
	payment_method_id int4 NOT NULL,
	CONSTRAINT sale_hand_over_pkey PRIMARY KEY (ho_id),
	CONSTRAINT sale_hand_over_company_id FOREIGN KEY (company_id) REFERENCES public.company(company_id) ON DELETE RESTRICT,
	CONSTRAINT sale_hand_over_customer_id FOREIGN KEY (customer_id) REFERENCES public.customer(customer_id) ON DELETE RESTRICT,
	CONSTRAINT sale_hand_over_pmethod_id FOREIGN KEY (payment_method_id) REFERENCES public.payment(payment_id) ON DELETE RESTRICT,
	CONSTRAINT sale_hand_over_purse_id FOREIGN KEY (purse_id) REFERENCES public.purse(purse_id) ON DELETE RESTRICT,
	CONSTRAINT sale_hand_over_user_hand_id FOREIGN KEY (user_hand_id) REFERENCES public."user"(user_id) ON DELETE RESTRICT,
	CONSTRAINT sale_hand_over_user_id FOREIGN KEY (user_id) REFERENCES public."user"(user_id) ON DELETE RESTRICT,
	CONSTRAINT sale_hand_over_user_ret_id FOREIGN KEY (user_ret_id) REFERENCES public."user"(user_id) ON DELETE RESTRICT,
	CONSTRAINT sale_hand_over_warehouse_id FOREIGN KEY (warehouse_id) REFERENCES public.warehouse(warehouse_id) ON DELETE RESTRICT
);
CREATE INDEX sale_hand_over_amount_idx ON public.sale_hand_over USING btree (amount);
CREATE INDEX sale_hand_over_company_id_idx ON public.sale_hand_over USING btree (company_id);
CREATE INDEX sale_hand_over_created_idx ON public.sale_hand_over USING btree (created);
CREATE INDEX sale_hand_over_customer_id_idx ON public.sale_hand_over USING btree (customer_id);
CREATE INDEX sale_hand_over_handed_idx ON public.sale_hand_over USING btree (handed);
CREATE INDEX sale_hand_over_payment_method_id_idx ON public.sale_hand_over USING btree (payment_method_id);
CREATE INDEX sale_hand_over_purse_id_idx ON public.sale_hand_over USING btree (purse_id);
CREATE INDEX sale_hand_over_returned_idx ON public.sale_hand_over USING btree (returned);
CREATE INDEX sale_hand_over_user_hand_id_idx ON public.sale_hand_over USING btree (user_hand_id);
CREATE INDEX sale_hand_over_user_id_idx ON public.sale_hand_over USING btree (user_id);
CREATE INDEX sale_hand_over_user_ret_id_idx ON public.sale_hand_over USING btree (user_ret_id);
CREATE INDEX sale_hand_over_warehouse_id_idx ON public.sale_hand_over USING btree (warehouse_id);


-- public.sale_payment definition

-- Drop table

-- DROP TABLE public.sale_payment;

CREATE TABLE public.sale_payment (
	sale_payment_id serial4 NOT NULL,
	sale_id int4 NOT NULL,
	invoice_id int4 NULL,
	purse_id int4 NULL,
	amount numeric(12, 4) NOT NULL,
	amount_currency_id int2 NOT NULL,
	payment_id int2 NOT NULL,
	"comment" text NULL,
	state int4 DEFAULT 0 NOT NULL,
	event_id jsonb DEFAULT '[]'::jsonb NOT NULL,
	"date" timestamptz DEFAULT now() NOT NULL,
	paid_date timestamptz NULL,
	CONSTRAINT sale_payment_pkey PRIMARY KEY (sale_payment_id),
	CONSTRAINT sale_payment_sale_id_fkey FOREIGN KEY (sale_id) REFERENCES public.sale(sale_id) ON DELETE CASCADE
);
CREATE INDEX sale_payment_sale_id ON public.sale_payment USING btree (sale_id);


-- public.sale_return_data definition

-- Drop table

-- DROP TABLE public.sale_return_data;

CREATE TABLE public.sale_return_data (
	sale_return_data_id serial4 NOT NULL,
	sale_data_id int4 NOT NULL,
	CONSTRAINT sale_return_data_pkey PRIMARY KEY (sale_return_data_id),
	CONSTRAINT sale_return_data_sale_data_id_fkey FOREIGN KEY (sale_data_id) REFERENCES public.sale_data(sale_data_id) ON DELETE RESTRICT
);
CREATE INDEX sale_return_data_sale_data_id ON public.sale_return_data USING btree (sale_data_id);


-- public.sale_type definition

-- Drop table

-- DROP TABLE public.sale_type;

CREATE TABLE public.sale_type (
	sale_type_id serial4 NOT NULL,
	tpl_sale_id int4 NULL,
	description varchar(100) NULL,
	attachment varchar(50) NULL,
	attachment_en varchar(50) NULL,
	tit_attachment varchar(50) NULL,
	tit_attachment_en varchar(50) NULL,
	intl jsonb DEFAULT '{}'::jsonb NOT NULL,
	CONSTRAINT sale_type_pkey PRIMARY KEY (sale_type_id),
	CONSTRAINT sale_type_tpl_sale_id FOREIGN KEY (tpl_sale_id) REFERENCES public.sale(sale_id) ON DELETE SET NULL
);


-- public.sale_wh_amount definition

-- Drop table

-- DROP TABLE public.sale_wh_amount;

CREATE TABLE public.sale_wh_amount (
	sale_id int4 NOT NULL,
	"percent" int2 DEFAULT 0 NOT NULL,
	open_time timestamptz DEFAULT now() NOT NULL,
	wait_months int4 DEFAULT 0 NOT NULL,
	CONSTRAINT sale_wh_amount_sale_id_fkey FOREIGN KEY (sale_id) REFERENCES public.sale(sale_id) ON DELETE CASCADE
);
CREATE INDEX sale_wh_amount_open_time_idx ON public.sale_wh_amount USING btree (open_time);
CREATE INDEX sale_wh_amount_sale_id_idx ON public.sale_wh_amount USING btree (sale_id);
CREATE INDEX sale_wh_amount_wait_months_idx ON public.sale_wh_amount USING btree (wait_months);


-- public.sl_req_import definition

-- Drop table

-- DROP TABLE public.sl_req_import;

CREATE TABLE public.sl_req_import (
	ri_id bigserial NOT NULL,
	sale_type_id int4 NULL,
	company_id int4 NULL,
	customer_id int4 NULL,
	source_id int4 NULL,
	user_comment text NULL,
	created_id int4 NOT NULL,
	modified_id int4 NOT NULL,
	created timestamptz DEFAULT now() NOT NULL,
	modified timestamptz DEFAULT now() NOT NULL,
	trader_ids jsonb DEFAULT '[]'::jsonb NOT NULL,
	CONSTRAINT sl_req_import_pkey PRIMARY KEY (ri_id),
	CONSTRAINT sl_req_import_company_id_id_fkey FOREIGN KEY (company_id) REFERENCES public.company(company_id) ON DELETE RESTRICT,
	CONSTRAINT sl_req_import_created_id_fkey FOREIGN KEY (created_id) REFERENCES public."user"(user_id) ON DELETE RESTRICT,
	CONSTRAINT sl_req_import_customer_id_fkey FOREIGN KEY (customer_id) REFERENCES public.customer(customer_id) ON DELETE RESTRICT,
	CONSTRAINT sl_req_import_modified_id_fkey FOREIGN KEY (modified_id) REFERENCES public."user"(user_id) ON DELETE RESTRICT,
	CONSTRAINT sl_req_import_sale_type_id_fkey FOREIGN KEY (sale_type_id) REFERENCES public.sale_type(sale_type_id) ON DELETE RESTRICT,
	CONSTRAINT sl_req_import_source_id_fkey FOREIGN KEY (source_id) REFERENCES public.sale_source(source_id) ON DELETE RESTRICT
);
CREATE INDEX sl_req_import_company_id_idx ON public.sl_req_import USING btree (company_id);
CREATE INDEX sl_req_import_created_id_idx ON public.sl_req_import USING btree (created_id);
CREATE INDEX sl_req_import_created_idx ON public.sl_req_import USING btree (created);
CREATE INDEX sl_req_import_customer_id_idx ON public.sl_req_import USING btree (customer_id);
CREATE INDEX sl_req_import_modified_id_idx ON public.sl_req_import USING btree (modified_id);
CREATE INDEX sl_req_import_modified_idx ON public.sl_req_import USING btree (modified);
CREATE INDEX sl_req_import_sale_type_id_idx ON public.sl_req_import USING btree (sale_type_id);
CREATE INDEX sl_req_import_source_id_idx ON public.sl_req_import USING btree (source_id);
CREATE INDEX sl_req_import_trader_ids_idx ON public.sl_req_import USING btree (trader_ids);


-- public.sl_req_offer definition

-- Drop table

-- DROP TABLE public.sl_req_offer;

CREATE TABLE public.sl_req_offer (
	req_id bigserial NOT NULL,
	sale_type_id int4 NULL,
	company_id int4 NULL,
	customer_id int4 NULL,
	supplier_id int4 NULL,
	offer_id int4 NULL,
	purchase_id int4 NULL,
	source_id int4 NULL,
	user_comment text NULL,
	created_id int4 NOT NULL,
	modified_id int4 NOT NULL,
	created timestamptz DEFAULT now() NOT NULL,
	modified timestamptz DEFAULT now() NOT NULL,
	trader_ids jsonb DEFAULT '[]'::jsonb NOT NULL,
	"locked" bool DEFAULT false NOT NULL,
	CONSTRAINT sl_req_offer_pkey PRIMARY KEY (req_id),
	CONSTRAINT sl_req_offer_company_id_id_fkey FOREIGN KEY (company_id) REFERENCES public.company(company_id) ON DELETE RESTRICT,
	CONSTRAINT sl_req_offer_created_id_fkey FOREIGN KEY (created_id) REFERENCES public."user"(user_id) ON DELETE RESTRICT,
	CONSTRAINT sl_req_offer_customer_id_fkey FOREIGN KEY (customer_id) REFERENCES public.customer(customer_id) ON DELETE RESTRICT,
	CONSTRAINT sl_req_offer_modified_id_fkey FOREIGN KEY (modified_id) REFERENCES public."user"(user_id) ON DELETE RESTRICT,
	CONSTRAINT sl_req_offer_offer_id_fkey FOREIGN KEY (offer_id) REFERENCES public.sale(sale_id) ON DELETE SET NULL,
	CONSTRAINT sl_req_offer_sale_type_id_fkey FOREIGN KEY (sale_type_id) REFERENCES public.sale_type(sale_type_id) ON DELETE RESTRICT,
	CONSTRAINT sl_req_offer_source_id_fkey FOREIGN KEY (source_id) REFERENCES public.sale_source(source_id) ON DELETE RESTRICT,
	CONSTRAINT sl_req_offer_supplier_id_fkey FOREIGN KEY (supplier_id) REFERENCES public.supplier(supplier_id) ON DELETE RESTRICT
);
CREATE INDEX sl_req_offer_company_id_idx ON public.sl_req_offer USING btree (company_id);
CREATE INDEX sl_req_offer_created_id_idx ON public.sl_req_offer USING btree (created_id);
CREATE INDEX sl_req_offer_created_idx ON public.sl_req_offer USING btree (created);
CREATE INDEX sl_req_offer_customer_id_idx ON public.sl_req_offer USING btree (customer_id);
CREATE INDEX sl_req_offer_locked_idx ON public.sl_req_offer USING btree (locked);
CREATE INDEX sl_req_offer_modified_id_idx ON public.sl_req_offer USING btree (modified_id);
CREATE INDEX sl_req_offer_modified_idx ON public.sl_req_offer USING btree (modified);
CREATE INDEX sl_req_offer_offer_id_idx ON public.sl_req_offer USING btree (offer_id);
CREATE INDEX sl_req_offer_purchase_id_idx ON public.sl_req_offer USING btree (purchase_id);
CREATE INDEX sl_req_offer_sale_type_id_idx ON public.sl_req_offer USING btree (sale_type_id);
CREATE INDEX sl_req_offer_source_id_idx ON public.sl_req_offer USING btree (source_id);
CREATE INDEX sl_req_offer_supplier_id_idx ON public.sl_req_offer USING btree (supplier_id);
CREATE INDEX sl_req_offer_trader_ids_idx ON public.sl_req_offer USING btree (trader_ids);


-- public.sl_req_offer_data definition

-- Drop table

-- DROP TABLE public.sl_req_offer_data;

CREATE TABLE public.sl_req_offer_data (
	data_id bigserial NOT NULL,
	req_id int8 NOT NULL,
	product_id int4 NULL,
	quantity numeric(12, 4) DEFAULT 0 NOT NULL,
	price_in numeric(12, 4) DEFAULT 0 NOT NULL,
	currency_id int2 NOT NULL,
	price_out numeric(12, 4) DEFAULT 0 NOT NULL,
	"name" text NULL,
	"position" int4 DEFAULT 0 NOT NULL,
	CONSTRAINT sl_req_offer_data_pkey PRIMARY KEY (data_id),
	CONSTRAINT sl_req_offer_data_req_id_fkey FOREIGN KEY (req_id) REFERENCES public.sl_req_offer(req_id) ON DELETE CASCADE
);
CREATE INDEX sl_req_offer_data_position_idx ON public.sl_req_offer_data USING btree ("position");
CREATE INDEX sl_req_offer_data_req_id_idx ON public.sl_req_offer_data USING btree (req_id);


-- public.stats_cust_money_balance definition

-- Drop table

-- DROP TABLE public.stats_cust_money_balance;

CREATE TABLE public.stats_cust_money_balance (
	bal_id serial4 NOT NULL,
	creator_id int4 NOT NULL,
	"name" varchar(90) NULL,
	stamp timestamptz DEFAULT now() NOT NULL,
	calc_stamp timestamptz NULL,
	param jsonb NULL,
	results jsonb NULL,
	CONSTRAINT stats_cust_money_balance_pkey PRIMARY KEY (bal_id),
	CONSTRAINT stats_cust_money_balance_creator_id FOREIGN KEY (creator_id) REFERENCES public."user"(user_id) ON DELETE RESTRICT
);
CREATE INDEX stats_cust_money_balance_calc_stamp_idx ON public.stats_cust_money_balance USING btree (calc_stamp);
CREATE INDEX stats_cust_money_balance_creator_id_idx ON public.stats_cust_money_balance USING btree (creator_id);
CREATE INDEX stats_cust_money_balance_name_idx ON public.stats_cust_money_balance USING btree (name);
CREATE INDEX stats_cust_money_balance_stamp_idx ON public.stats_cust_money_balance USING btree (stamp);


-- public.support_request definition

-- Drop table

-- DROP TABLE public.support_request;

CREATE TABLE public.support_request (
	request_id serial4 NOT NULL,
	user_id int4 NULL,
	cat_id int4 NULL,
	type_id int4 NULL,
	task_id int4 NULL,
	title text NULL,
	customer_id int4 NULL,
	machine_id int4 NULL,
	user_name varchar(60) NULL,
	office varchar(60) NULL,
	descr text NULL,
	req_stamp timestamptz DEFAULT now() NOT NULL,
	desired_stamp timestamptz NULL,
	work_start_stamp timestamptz NULL,
	work_end_stamp timestamptz NULL,
	answer_stamp timestamptz NULL,
	"offset" int4 DEFAULT 0 NOT NULL,
	responsible jsonb DEFAULT '[]'::jsonb NOT NULL,
	state int2 DEFAULT 0 NOT NULL,
	"read" bool DEFAULT false NOT NULL,
	approved bool DEFAULT false NOT NULL,
	needs_offer bool DEFAULT false NOT NULL,
	offer_ids jsonb DEFAULT '[]'::jsonb NOT NULL,
	assoc_id int4 NULL,
	proto_data jsonb DEFAULT '{}'::jsonb NOT NULL,
	wt_id int4 NULL,
	modified_id int4 NULL,
	modified timestamptz NULL,
	CONSTRAINT support_request_pkey PRIMARY KEY (request_id),
	CONSTRAINT support_request_request_id_key UNIQUE (request_id),
	CONSTRAINT support_request_cat_id_fkey FOREIGN KEY (cat_id) REFERENCES public.support_cat(cat_id) ON DELETE SET NULL,
	CONSTRAINT support_request_type_id_fkey FOREIGN KEY (type_id) REFERENCES public.support_type(type_id) ON DELETE SET NULL,
	CONSTRAINT support_request_wt_id_fkey FOREIGN KEY (wt_id) REFERENCES public.support_w_type(wt_id) ON DELETE SET NULL
);
CREATE INDEX support_request_answer_stamp ON public.support_request USING btree (answer_stamp);
CREATE INDEX support_request_approved ON public.support_request USING btree (approved);
CREATE INDEX support_request_cat_id ON public.support_request USING btree (cat_id);
CREATE INDEX support_request_customer_id ON public.support_request USING btree (customer_id);
CREATE INDEX support_request_end_stamp ON public.support_request USING btree (work_end_stamp);
CREATE INDEX support_request_machine_id ON public.support_request USING btree (machine_id);
CREATE INDEX support_request_modified ON public.support_request USING btree (modified);
CREATE INDEX support_request_modified_id ON public.support_request USING btree (modified_id);
CREATE INDEX support_request_needs_assoc_id ON public.support_request USING btree (assoc_id);
CREATE INDEX support_request_needs_offer ON public.support_request USING btree (needs_offer);
CREATE INDEX support_request_offer_ids ON public.support_request USING btree (offer_ids);
CREATE INDEX support_request_read ON public.support_request USING btree (read);
CREATE INDEX support_request_req_stamp ON public.support_request USING btree (req_stamp);
CREATE INDEX support_request_responsible ON public.support_request USING btree (responsible);
CREATE INDEX support_request_start_stamp ON public.support_request USING btree (work_start_stamp);
CREATE INDEX support_request_state ON public.support_request USING btree (state);
CREATE INDEX support_request_title ON public.support_request USING btree (title);
CREATE INDEX support_request_type_id ON public.support_request USING btree (type_id);
CREATE INDEX support_request_user_id ON public.support_request USING btree (user_id);
CREATE INDEX support_request_wt_id ON public.support_request USING btree (wt_id);


-- public.support_request_offer_time definition

-- Drop table

-- DROP TABLE public.support_request_offer_time;

CREATE TABLE public.support_request_offer_time (
	ro_id bigserial NOT NULL,
	request_id int4 NOT NULL,
	stamp timestamptz DEFAULT now() NOT NULL,
	ttime int4 DEFAULT 0 NOT NULL,
	ttype int2 DEFAULT 0 NOT NULL,
	user_id int4 NOT NULL,
	CONSTRAINT support_request_offer_time_pkey PRIMARY KEY (ro_id),
	CONSTRAINT support_request_offer_time_request_id_fkey FOREIGN KEY (request_id) REFERENCES public.support_request(request_id) ON DELETE CASCADE
);
CREATE INDEX support_request_offer_time_request_id_idx ON public.support_request_offer_time USING btree (request_id);
CREATE INDEX support_request_offer_time_stamp_idx ON public.support_request_offer_time USING btree (stamp);
CREATE INDEX support_request_offer_time_user_id_idx ON public.support_request_offer_time USING btree (user_id);


-- public.auth_mail_attach definition

-- Drop table

-- DROP TABLE public.auth_mail_attach;

CREATE TABLE public.auth_mail_attach (
	att_id serial4 NOT NULL,
	msg_id int4 NOT NULL,
	"source" varchar(90) NULL,
	CONSTRAINT auth_mail_attach_pkey PRIMARY KEY (att_id),
	CONSTRAINT auth_mail_attach_msg_id_fkey FOREIGN KEY (msg_id) REFERENCES public.auth_mail_message(msg_id) ON DELETE CASCADE
);
CREATE INDEX auth_mail_attach_msg_id_idx ON public.auth_mail_attach USING btree (msg_id);


-- public.case_archive_book definition

-- Drop table

-- DROP TABLE public.case_archive_book;

CREATE TABLE public.case_archive_book (
	ab_id bigserial NOT NULL,
	case_id int8 NOT NULL,
	arch_date timestamptz DEFAULT '1900-01-01 00:00:00+02'::timestamp with time zone NOT NULL,
	link_no int4 NULL,
	link_date timestamptz DEFAULT '1900-01-01 00:00:00+02'::timestamp with time zone NOT NULL,
	proto_no int4 NULL,
	doc_saved text NULL,
	vol_year int2 DEFAULT 1900 NOT NULL,
	vol_no int4 DEFAULT 0 NOT NULL,
	notes text NULL,
	book_nr int8 NULL,
	CONSTRAINT case_archive_book_pkey PRIMARY KEY (ab_id),
	CONSTRAINT case_archive_book_case_id_fkey FOREIGN KEY (case_id) REFERENCES public.case_case(case_id) ON DELETE CASCADE
);
CREATE INDEX case_archive_book_arch_date_idx ON public.case_archive_book USING btree (arch_date);
CREATE INDEX case_archive_book_book_nr_idx ON public.case_archive_book USING btree (book_nr);
CREATE INDEX case_archive_book_case_id_idx ON public.case_archive_book USING btree (case_id);
CREATE INDEX case_archive_book_doc_saved_idx ON public.case_archive_book USING btree (doc_saved);
CREATE INDEX case_archive_book_link_date_idx ON public.case_archive_book USING btree (link_date);
CREATE INDEX case_archive_book_link_no_idx ON public.case_archive_book USING btree (link_no);
CREATE INDEX case_archive_book_proto_no_idx ON public.case_archive_book USING btree (proto_no);
CREATE INDEX case_archive_book_vol_no_idx ON public.case_archive_book USING btree (vol_no);
CREATE INDEX case_archive_book_vol_year_idx ON public.case_archive_book USING btree (vol_year);


-- public.case_arrived_money definition

-- Drop table

-- DROP TABLE public.case_arrived_money;

CREATE TABLE public.case_arrived_money (
	arr_id serial4 NOT NULL,
	amount numeric(12, 3) DEFAULT 0::numeric NOT NULL,
	amount_left numeric(12, 3) DEFAULT 0::numeric NOT NULL,
	currency_id int4 DEFAULT 1 NOT NULL,
	stamp timestamptz DEFAULT now() NOT NULL,
	person_id int4 NULL,
	reason text NULL,
	bank_id int2 DEFAULT '-1'::integer NOT NULL,
	case_id int4 NULL,
	operation_id varchar(90) NULL,
	iban varchar(30) NULL,
	created_id int4 NULL,
	modified_id int4 NULL,
	created timestamptz DEFAULT now() NOT NULL,
	modified timestamptz DEFAULT now() NOT NULL,
	sender_bank_code varchar(30) NULL,
	sender_iban varchar(30) NULL,
	sender_id int8 NULL,
	sender_name varchar(180) NULL,
	invoice_amount numeric(12, 3) DEFAULT 0 NOT NULL,
	account_amount numeric(12, 3) DEFAULT 0 NOT NULL,
	arrived_amount numeric(12, 3) DEFAULT '0'::numeric NOT NULL,
	type_id int2 DEFAULT 0 NOT NULL,
	invoice_id int8 NULL,
	CONSTRAINT case_arrived_money_pkey PRIMARY KEY (arr_id),
	CONSTRAINT case_arrived_money_case_id_fkey FOREIGN KEY (case_id) REFERENCES public.case_case(case_id) ON DELETE RESTRICT,
	CONSTRAINT case_arrived_money_created_id FOREIGN KEY (created_id) REFERENCES public."user"(user_id) ON DELETE RESTRICT,
	CONSTRAINT case_arrived_money_modified_id FOREIGN KEY (modified_id) REFERENCES public."user"(user_id) ON DELETE RESTRICT,
	CONSTRAINT case_arrived_money_person_id_fkey FOREIGN KEY (person_id) REFERENCES public.case_person(person_id) ON DELETE RESTRICT,
	CONSTRAINT case_arrived_money_sender_id_fkey FOREIGN KEY (sender_id) REFERENCES public.case_person(person_id) ON DELETE RESTRICT
);
CREATE INDEX case_arrived_money_account_amount_idx ON public.case_arrived_money USING btree (account_amount);
CREATE INDEX case_arrived_money_amount_idx ON public.case_arrived_money USING btree (amount);
CREATE INDEX case_arrived_money_amount_left_idx ON public.case_arrived_money USING btree (amount_left);
CREATE INDEX case_arrived_money_bank_id_idx ON public.case_arrived_money USING btree (bank_id);
CREATE INDEX case_arrived_money_case_id_idx ON public.case_arrived_money USING btree (case_id);
CREATE INDEX case_arrived_money_created_id_idx ON public.case_arrived_money USING btree (created_id);
CREATE INDEX case_arrived_money_created_idx ON public.case_arrived_money USING btree (created);
CREATE INDEX case_arrived_money_iban_idx ON public.case_arrived_money USING btree (iban);
CREATE INDEX case_arrived_money_invoice_amount_idx ON public.case_arrived_money USING btree (invoice_amount);
CREATE INDEX case_arrived_money_invoice_id_idx ON public.case_arrived_money USING btree (invoice_id);
CREATE INDEX case_arrived_money_modified_id_idx ON public.case_arrived_money USING btree (modified_id);
CREATE INDEX case_arrived_money_modified_idx ON public.case_arrived_money USING btree (modified);
CREATE INDEX case_arrived_money_operation_id_idx ON public.case_arrived_money USING btree (operation_id);
CREATE INDEX case_arrived_money_person_id_idx ON public.case_arrived_money USING btree (person_id);
CREATE INDEX case_arrived_money_reason_idx ON public.case_arrived_money USING btree (reason);
CREATE INDEX case_arrived_money_sender_bank_code_idx ON public.case_arrived_money USING btree (sender_bank_code);
CREATE INDEX case_arrived_money_sender_iban_idx ON public.case_arrived_money USING btree (sender_iban);
CREATE INDEX case_arrived_money_sender_id_idx ON public.case_arrived_money USING btree (sender_id);
CREATE INDEX case_arrived_money_sender_name_idx ON public.case_arrived_money USING btree (sender_name);
CREATE INDEX case_arrived_money_stamp_idx ON public.case_arrived_money USING btree (stamp);
CREATE INDEX case_arrived_money_type_id_idx ON public.case_arrived_money USING btree (type_id);


-- public.case_arrived_money_transfer definition

-- Drop table

-- DROP TABLE public.case_arrived_money_transfer;

CREATE TABLE public.case_arrived_money_transfer (
	tr_id bigserial NOT NULL,
	arr_id int8 NOT NULL,
	from_case_id int8 NOT NULL,
	to_case_id int8 NOT NULL,
	stamp timestamptz DEFAULT now() NOT NULL,
	user_id int4 NOT NULL,
	amount numeric(12, 3) DEFAULT 0 NOT NULL,
	currency_id int2 DEFAULT 1 NOT NULL,
	CONSTRAINT case_arrived_money_transfer_pkey PRIMARY KEY (tr_id),
	CONSTRAINT case_arrived_money_transfer FOREIGN KEY (user_id) REFERENCES public."user"(user_id) ON DELETE RESTRICT,
	CONSTRAINT case_arrived_money_transfer_arr_id_fkey FOREIGN KEY (arr_id) REFERENCES public.case_arrived_money(arr_id) ON DELETE CASCADE,
	CONSTRAINT case_arrived_money_transfer_from_case_id_fkey FOREIGN KEY (from_case_id) REFERENCES public.case_case(case_id) ON DELETE CASCADE,
	CONSTRAINT case_arrived_money_transfer_to_case_id_fkey FOREIGN KEY (to_case_id) REFERENCES public.case_case(case_id) ON DELETE CASCADE
);
CREATE INDEX case_arrived_money_transfer_amount_idx ON public.case_arrived_money_transfer USING btree (amount);
CREATE INDEX case_arrived_money_transfer_arr_id_idx ON public.case_arrived_money_transfer USING btree (arr_id);
CREATE INDEX case_arrived_money_transfer_from_case_id_idx ON public.case_arrived_money_transfer USING btree (from_case_id);
CREATE INDEX case_arrived_money_transfer_stamp_idx ON public.case_arrived_money_transfer USING btree (stamp);
CREATE INDEX case_arrived_money_transfer_to_case_id_idx ON public.case_arrived_money_transfer USING btree (to_case_id);
CREATE INDEX case_arrived_money_transfer_user_id_idx ON public.case_arrived_money_transfer USING btree (user_id);


-- public.case_bnb_bank_account definition

-- Drop table

-- DROP TABLE public.case_bnb_bank_account;

CREATE TABLE public.case_bnb_bank_account (
	acc_id serial4 NOT NULL,
	person_id int4 NOT NULL,
	case_id int4 NOT NULL,
	open_date timestamptz NOT NULL,
	close_date timestamptz NULL,
	doc_date timestamptz NOT NULL,
	stamp timestamptz DEFAULT now() NOT NULL,
	iban varchar(90) NULL,
	type_code int2 DEFAULT 0 NOT NULL,
	"type" varchar(120) NULL,
	group_code int2 DEFAULT 0 NOT NULL,
	"group" varchar(120) NULL,
	role_code int2 DEFAULT 0 NOT NULL,
	"role" varchar(120) NULL,
	bank_id int2 DEFAULT '-1'::integer NOT NULL,
	bank_name text NULL,
	currency_id int2 DEFAULT 1 NOT NULL,
	att jsonb DEFAULT '[]'::jsonb NOT NULL,
	"real" bool DEFAULT true NOT NULL,
	CONSTRAINT case_bnb_bank_account_pkey PRIMARY KEY (acc_id),
	CONSTRAINT case_bnb_bank_account_case_id_fkey FOREIGN KEY (case_id) REFERENCES public.case_case(case_id) ON DELETE RESTRICT,
	CONSTRAINT case_bnb_bank_account_person_id_fkey FOREIGN KEY (person_id) REFERENCES public.case_person(person_id) ON DELETE RESTRICT
);
CREATE INDEX case_bnb_bank_account_case_id_idx ON public.case_bnb_bank_account USING btree (case_id);
CREATE INDEX case_bnb_bank_account_person_id_idx ON public.case_bnb_bank_account USING btree (person_id);
CREATE INDEX case_bnb_bank_account_real_idx ON public.case_bnb_bank_account USING btree ("real");


-- public.case_bnb_check definition

-- Drop table

-- DROP TABLE public.case_bnb_check;

CREATE TABLE public.case_bnb_check (
	chk_id serial4 NOT NULL,
	person_id int4 NOT NULL,
	case_id int4 NOT NULL,
	stamp timestamptz DEFAULT now() NOT NULL,
	exp_stamp timestamptz NULL,
	CONSTRAINT case_bnb_check_pkey PRIMARY KEY (chk_id),
	CONSTRAINT case_bnb_check_case_id_fkey FOREIGN KEY (case_id) REFERENCES public.case_case(case_id) ON DELETE RESTRICT,
	CONSTRAINT case_bnb_check_person_id_fkey FOREIGN KEY (person_id) REFERENCES public.case_person(person_id) ON DELETE RESTRICT
);
CREATE INDEX case_bnb_check_case_id_idx ON public.case_bnb_check USING btree (case_id);
CREATE INDEX case_bnb_check_exp_stamp_idx ON public.case_bnb_check USING btree (exp_stamp);
CREATE INDEX case_bnb_check_person_id_idx ON public.case_bnb_check USING btree (person_id);
CREATE INDEX case_bnb_check_stamp_idx ON public.case_bnb_check USING btree (stamp);


-- public.case_complex definition

-- Drop table

-- DROP TABLE public.case_complex;

CREATE TABLE public.case_complex (
	cplx_id serial4 NOT NULL,
	country_id int4 NOT NULL,
	zone_id int4 NOT NULL,
	mun_id int4 NOT NULL,
	town_id int4 NOT NULL,
	qtr_id int4 NULL,
	"name" varchar(120) NOT NULL,
	CONSTRAINT case_complex_pkey PRIMARY KEY (cplx_id),
	CONSTRAINT case_complex_mun_id_fkey FOREIGN KEY (mun_id) REFERENCES public.case_municipality(mun_id) ON DELETE RESTRICT,
	CONSTRAINT case_complex_qtr_id_fkey FOREIGN KEY (qtr_id) REFERENCES public.case_quarter(qtr_id) ON DELETE RESTRICT,
	CONSTRAINT case_complex_town_id_fkey FOREIGN KEY (town_id) REFERENCES public.case_town(town_id) ON DELETE RESTRICT
);
CREATE INDEX case_complex_country_id_idx ON public.case_complex USING btree (country_id);
CREATE INDEX case_complex_mun_id_idx ON public.case_complex USING btree (mun_id);
CREATE INDEX case_complex_name_idx ON public.case_complex USING btree (name);
CREATE INDEX case_complex_qtr_id_idx ON public.case_complex USING btree (qtr_id);
CREATE INDEX case_complex_town_id_idx ON public.case_complex USING btree (town_id);
CREATE INDEX case_complex_zone_id_idx ON public.case_complex USING btree (zone_id);


-- public.case_street definition

-- Drop table

-- DROP TABLE public.case_street;

CREATE TABLE public.case_street (
	str_id serial4 NOT NULL,
	country_id int4 NOT NULL,
	zone_id int4 NOT NULL,
	mun_id int4 NOT NULL,
	town_id int4 NOT NULL,
	qtr_id int4 NULL,
	cplx_id int4 NULL,
	"name" varchar(120) NOT NULL,
	"type" int2 DEFAULT 0 NOT NULL,
	CONSTRAINT case_street_pkey PRIMARY KEY (str_id),
	CONSTRAINT case_street_cplx_id_fkey FOREIGN KEY (cplx_id) REFERENCES public.case_complex(cplx_id) ON DELETE RESTRICT,
	CONSTRAINT case_street_mun_id_fkey FOREIGN KEY (mun_id) REFERENCES public.case_municipality(mun_id) ON DELETE RESTRICT,
	CONSTRAINT case_street_qtr_id_fkey FOREIGN KEY (qtr_id) REFERENCES public.case_quarter(qtr_id) ON DELETE RESTRICT,
	CONSTRAINT case_street_town_id_fkey FOREIGN KEY (town_id) REFERENCES public.case_town(town_id) ON DELETE RESTRICT
);
CREATE INDEX case_street_country_id_idx ON public.case_street USING btree (country_id);
CREATE INDEX case_street_cplx_id_idx ON public.case_street USING btree (cplx_id);
CREATE INDEX case_street_mun_id_idx ON public.case_street USING btree (mun_id);
CREATE INDEX case_street_name_idx ON public.case_street USING btree (name);
CREATE INDEX case_street_qtr_id_idx ON public.case_street USING btree (qtr_id);
CREATE INDEX case_street_town_id_idx ON public.case_street USING btree (town_id);
CREATE INDEX case_street_zone_id_idx ON public.case_street USING btree (zone_id);


-- public.cases_bnb_process definition

-- Drop table

-- DROP TABLE public.cases_bnb_process;

CREATE TABLE public.cases_bnb_process (
	proc_id bigserial NOT NULL,
	case_id int8 NOT NULL,
	person_id int8 NOT NULL,
	chk_id int8 NOT NULL,
	created timestamptz DEFAULT now() NOT NULL,
	exported timestamptz NULL,
	received timestamptz NULL,
	has_account bool DEFAULT false NOT NULL,
	is_active bool DEFAULT false NOT NULL,
	is_distrainted bool DEFAULT false NOT NULL,
	distrainted timestamptz NULL,
	bank_id int4 NULL,
	distraint_confirmed timestamptz NULL,
	distrainted_deed_id int8 NULL,
	raised timestamptz NULL,
	raise_confirmed timestamptz NULL,
	CONSTRAINT cases_bnb_process_pkey PRIMARY KEY (proc_id),
	CONSTRAINT cases_bnb_process_case_id_fkey FOREIGN KEY (case_id) REFERENCES public.case_case(case_id) ON DELETE CASCADE,
	CONSTRAINT cases_bnb_process_chk_id_fkey FOREIGN KEY (chk_id) REFERENCES public.case_bnb_check(chk_id) ON DELETE RESTRICT,
	CONSTRAINT cases_bnb_process_distrainted_deed_id_fkey FOREIGN KEY (distrainted_deed_id) REFERENCES public.case_case_deed(deed_id) ON DELETE SET NULL,
	CONSTRAINT cases_bnb_process_person_id_fkey FOREIGN KEY (person_id) REFERENCES public.case_person(person_id) ON DELETE CASCADE
);
CREATE INDEX cases_bnb_process_bank_id_idx ON public.cases_bnb_process USING btree (bank_id);
CREATE INDEX cases_bnb_process_case_id_idx ON public.cases_bnb_process USING btree (case_id);
CREATE INDEX cases_bnb_process_chk_id_idx ON public.cases_bnb_process USING btree (chk_id);
CREATE INDEX cases_bnb_process_created_idx ON public.cases_bnb_process USING btree (created);
CREATE INDEX cases_bnb_process_distraint_confirmed_idx ON public.cases_bnb_process USING btree (distraint_confirmed);
CREATE INDEX cases_bnb_process_distrainted_deed_id_idx ON public.cases_bnb_process USING btree (distrainted_deed_id);
CREATE INDEX cases_bnb_process_distrainted_idx ON public.cases_bnb_process USING btree (distrainted);
CREATE INDEX cases_bnb_process_exported_idx ON public.cases_bnb_process USING btree (exported);
CREATE INDEX cases_bnb_process_has_account_idx ON public.cases_bnb_process USING btree (has_account);
CREATE INDEX cases_bnb_process_is_active_idx ON public.cases_bnb_process USING btree (is_active);
CREATE INDEX cases_bnb_process_is_distrainted_idx ON public.cases_bnb_process USING btree (is_distrainted);
CREATE INDEX cases_bnb_process_person_id_idx ON public.cases_bnb_process USING btree (person_id);
CREATE INDEX cases_bnb_process_raise_confirmed_idx ON public.cases_bnb_process USING btree (raise_confirmed);
CREATE INDEX cases_bnb_process_raised_idx ON public.cases_bnb_process USING btree (raised);
CREATE INDEX cases_bnb_process_received_idx ON public.cases_bnb_process USING btree (received);


-- public.cases_post_code definition

-- Drop table

-- DROP TABLE public.cases_post_code;

CREATE TABLE public.cases_post_code (
	cd_id serial4 NOT NULL,
	code int4 NOT NULL,
	zone_id int4 NOT NULL,
	mun_id int4 NULL,
	town_id int4 NULL,
	qtr_id int4 NULL,
	cplx_id int4 NULL,
	str_id int4 NULL,
	CONSTRAINT cases_post_code_pkey PRIMARY KEY (cd_id),
	CONSTRAINT cases_post_code_cplx_id_fkey FOREIGN KEY (cplx_id) REFERENCES public.case_complex(cplx_id) ON DELETE CASCADE,
	CONSTRAINT cases_post_code_mun_id_fkey FOREIGN KEY (mun_id) REFERENCES public.case_municipality(mun_id) ON DELETE CASCADE,
	CONSTRAINT cases_post_code_qtr_id_fkey FOREIGN KEY (qtr_id) REFERENCES public.case_quarter(qtr_id) ON DELETE CASCADE,
	CONSTRAINT cases_post_code_str_id_fkey FOREIGN KEY (str_id) REFERENCES public.case_street(str_id) ON DELETE CASCADE,
	CONSTRAINT cases_post_code_town_id_fkey FOREIGN KEY (town_id) REFERENCES public.case_town(town_id) ON DELETE CASCADE,
	CONSTRAINT cases_post_code_zone_id_fkey FOREIGN KEY (zone_id) REFERENCES public.country_zone(country_zone_id) ON DELETE RESTRICT
);
CREATE INDEX cases_post_code_code_idx ON public.cases_post_code USING btree (code);
CREATE INDEX cases_post_code_cplx_id_idx ON public.cases_post_code USING btree (cplx_id);
CREATE INDEX cases_post_code_mun_id_idx ON public.cases_post_code USING btree (mun_id);
CREATE INDEX cases_post_code_qtr_id_idx ON public.cases_post_code USING btree (qtr_id);
CREATE INDEX cases_post_code_str_id_idx ON public.cases_post_code USING btree (str_id);
CREATE INDEX cases_post_code_town_id_idx ON public.cases_post_code USING btree (town_id);
CREATE INDEX cases_post_code_zone_id_idx ON public.cases_post_code USING btree (zone_id);


-- public.cl_salary_person_prod definition

-- Drop table

-- DROP TABLE public.cl_salary_person_prod;

CREATE TABLE public.cl_salary_person_prod (
	pp_id serial4 NOT NULL,
	worker_id int4 NOT NULL,
	product_id int4 NOT NULL,
	"name" text NULL,
	quantity numeric(12, 3) NULL,
	price numeric(12, 2) NULL,
	price_currency_id int2 DEFAULT 1 NOT NULL,
	tax_product_id int2 DEFAULT 0 NULL,
	serial_id int4 NULL,
	"position" int2 DEFAULT 1 NOT NULL,
	CONSTRAINT cl_salary_person_prod_pkey PRIMARY KEY (pp_id),
	CONSTRAINT cl_salary_person_prod_worker_id_fkey FOREIGN KEY (worker_id) REFERENCES public.cl_salary_worker(worker_id) ON DELETE RESTRICT
);
CREATE INDEX cl_salary_person_prod_position_idx ON public.cl_salary_person_prod USING btree ("position");
CREATE INDEX cl_salary_person_prod_worker_id_idx ON public.cl_salary_person_prod USING btree (worker_id);


-- public.invoice definition

-- Drop table

-- DROP TABLE public.invoice;

CREATE TABLE public.invoice (
	invoice_id serial4 NOT NULL,
	"document" int4 NULL,
	"quote" int4 NULL,
	customer_id int4 NOT NULL,
	company_id int4 NOT NULL,
	payment_id int4 NOT NULL,
	sale_id int4 NULL,
	sale_payment_id int4 NULL,
	user_id int4 NULL,
	state int2 DEFAULT 0 NOT NULL,
	discount numeric(12, 2) NULL,
	discount_type int2 NULL,
	"ref" int4 NULL,
	"date" timestamptz DEFAULT now() NOT NULL,
	"comment" text NULL,
	"locked" bool DEFAULT false NOT NULL,
	locked_id int4 NULL,
	bank_account_id int4 NULL,
	user_comment text NULL,
	vat_comment text NULL,
	parent_id int4 NULL,
	credit bool DEFAULT false NOT NULL,
	payment_delay int4 DEFAULT 0 NOT NULL,
	advance_payment bool DEFAULT false NOT NULL,
	ap_percentage numeric(12, 2) DEFAULT 0 NOT NULL,
	ap_type int4 DEFAULT 0 NOT NULL,
	lock_notify_ids jsonb DEFAULT '[]'::jsonb NOT NULL,
	change_notify_ids jsonb DEFAULT '[]'::jsonb NOT NULL,
	comment_notify_ids jsonb DEFAULT '[]'::jsonb NOT NULL,
	unpaid_notify_ids jsonb DEFAULT '[]'::jsonb NOT NULL,
	financial_ids jsonb DEFAULT '[]'::jsonb NOT NULL,
	status int4 DEFAULT 0 NOT NULL,
	is_paid int2 DEFAULT 0 NOT NULL,
	library_id int4 NULL,
	seq_id int4 DEFAULT 0 NOT NULL,
	div_id int4 NULL,
	amount numeric(12, 3) DEFAULT 0 NOT NULL,
	amount_paid numeric(12, 3) DEFAULT 0 NOT NULL,
	amount_currency_id int2 DEFAULT 1 NOT NULL,
	proforma_id int4 NULL,
	validity timestamptz DEFAULT now() NOT NULL,
	vat_stamp timestamptz DEFAULT now() NOT NULL,
	receiver text NULL,
	city_id int4 NULL,
	calc_advance bool DEFAULT true NOT NULL,
	mail_id int8 NULL,
	pdf text NULL,
	wh_percent int2 DEFAULT 0 NOT NULL,
	CONSTRAINT invoice_pkey PRIMARY KEY (invoice_id),
	CONSTRAINT invoice_city_id_fkey FOREIGN KEY (city_id) REFERENCES public.city_details(city_id) ON DELETE SET NULL,
	CONSTRAINT invoice_company_id_fkey FOREIGN KEY (company_id) REFERENCES public.company(company_id) ON DELETE RESTRICT,
	CONSTRAINT invoice_customer_id_fkey FOREIGN KEY (customer_id) REFERENCES public.customer(customer_id) ON DELETE RESTRICT,
	CONSTRAINT invoice_locked_id FOREIGN KEY (locked_id) REFERENCES public."user"(user_id) ON DELETE RESTRICT,
	CONSTRAINT invoice_proforma_id_fkey FOREIGN KEY (proforma_id) REFERENCES public.invoice(invoice_id) ON DELETE SET NULL,
	CONSTRAINT invoice_sale_id_fkey FOREIGN KEY (sale_id) REFERENCES public.sale(sale_id) ON DELETE RESTRICT,
	CONSTRAINT invoice_sale_payment_id_fkey FOREIGN KEY (sale_payment_id) REFERENCES public.sale_payment(sale_payment_id) ON DELETE RESTRICT,
	CONSTRAINT invoice_user_id FOREIGN KEY (user_id) REFERENCES public."user"(user_id) ON DELETE RESTRICT
);
CREATE INDEX invoice_amount_idx ON public.invoice USING btree (amount);
CREATE INDEX invoice_amount_paid_idx ON public.invoice USING btree (amount_paid);
CREATE INDEX invoice_calc_advance_idx ON public.invoice USING btree (calc_advance);
CREATE INDEX invoice_city_id_idx ON public.invoice USING btree (city_id);
CREATE INDEX invoice_company_id ON public.invoice USING btree (company_id);
CREATE UNIQUE INDEX invoice_company_id_document_key ON public.invoice USING btree (company_id, document, seq_id, date_trunc('year'::text, timezone('UTC'::text, date)));
CREATE UNIQUE INDEX invoice_company_id_quote_key ON public.invoice USING btree (company_id, quote, seq_id, date_trunc('year'::text, timezone('UTC'::text, date)));
CREATE INDEX invoice_customer_id ON public.invoice USING btree (customer_id);
CREATE INDEX invoice_div_id ON public.invoice USING btree (div_id);
CREATE INDEX invoice_is_paid ON public.invoice USING btree (is_paid);
CREATE INDEX invoice_mail_id_idx ON public.invoice USING btree (mail_id);
CREATE INDEX invoice_parent ON public.invoice USING btree (parent_id);
CREATE INDEX invoice_proforma_id_idx ON public.invoice USING btree (proforma_id);
CREATE INDEX invoice_receiver_idx ON public.invoice USING btree (receiver);
CREATE INDEX invoice_validity_idx ON public.invoice USING btree (validity);
CREATE INDEX invoice_vat_stamp_idx ON public.invoice USING btree (vat_stamp);

-- Table Triggers

create trigger invoice_insert before
insert
    on
    public.invoice for each row execute function invoice_insert();


-- public.invoice_attach definition

-- Drop table

-- DROP TABLE public.invoice_attach;

CREATE TABLE public.invoice_attach (
	invoice_attach_id serial4 NOT NULL,
	invoice_id int4 NOT NULL,
	"source" varchar(100) NOT NULL,
	CONSTRAINT invoice_attach_pkey PRIMARY KEY (invoice_attach_id),
	CONSTRAINT invoice_attach_invoice_id_fkey FOREIGN KEY (invoice_id) REFERENCES public.invoice(invoice_id) ON DELETE CASCADE
);
CREATE INDEX invoice_attach_invoice_id ON public.invoice_attach USING btree (invoice_id);
CREATE INDEX invoice_attach_source ON public.invoice_attach USING btree (source);


-- public.invoice_data definition

-- Drop table

-- DROP TABLE public.invoice_data;

CREATE TABLE public.invoice_data (
	invoice_data_id serial4 NOT NULL,
	invoice_id int4 NOT NULL,
	product_id int4 NULL,
	serial varchar(100) NULL,
	quantity numeric(12, 4) DEFAULT 0 NOT NULL,
	price numeric(12, 4) DEFAULT 0 NOT NULL,
	price_currency_id int2 NOT NULL,
	tax_product_id int2 NULL,
	"name" text NULL,
	"position" int4 DEFAULT 0 NOT NULL,
	CONSTRAINT invoice_data_pkey PRIMARY KEY (invoice_data_id),
	CONSTRAINT invoice_data_invoice_id_fkey FOREIGN KEY (invoice_id) REFERENCES public.invoice(invoice_id) ON DELETE CASCADE
);
CREATE INDEX invoice_data_invoice_id ON public.invoice_data USING btree (invoice_id);


-- public.invoice_note_data definition

-- Drop table

-- DROP TABLE public.invoice_note_data;

CREATE TABLE public.invoice_note_data (
	invoice_note_data_id serial4 NOT NULL,
	invoice_note_id int4 NOT NULL,
	sale_return_data_id int4 NOT NULL,
	"position" int4 DEFAULT 0 NOT NULL,
	CONSTRAINT invoice_note_data_pkey PRIMARY KEY (invoice_note_data_id),
	CONSTRAINT invoice_note_data_invoice_note_id_fkey FOREIGN KEY (invoice_note_id) REFERENCES public.invoice_note(invoice_note_id) ON DELETE CASCADE,
	CONSTRAINT invoice_note_data_sale_return_data_id_fkey FOREIGN KEY (sale_return_data_id) REFERENCES public.sale_return_data(sale_return_data_id) ON DELETE RESTRICT
);
CREATE INDEX invoice_note_data_invoice_note_id ON public.invoice_note_data USING btree (invoice_note_id);
CREATE INDEX invoice_note_data_sale_return_data_id ON public.invoice_note_data USING btree (sale_return_data_id);


-- public.invoice_recieve definition

-- Drop table

-- DROP TABLE public.invoice_recieve;

CREATE TABLE public.invoice_recieve (
	invoice_recieve_id serial4 NOT NULL,
	parent_id int4 NULL,
	"document" varchar(30) DEFAULT ''::character varying NOT NULL,
	supplier_id int4 NOT NULL,
	company_id int4 NOT NULL,
	payment_id int4 NOT NULL,
	purchase_ids jsonb DEFAULT '[]'::jsonb NOT NULL,
	purchase_payment_id int4 NULL,
	user_id int4 NULL,
	user_id_last int4 NULL,
	"comment" text NULL,
	state int2 DEFAULT 0 NOT NULL,
	discount numeric(12, 2) NULL,
	discount_type int2 NULL,
	"date" timestamptz DEFAULT now() NOT NULL,
	"locked" bool DEFAULT false NOT NULL,
	locked_id int4 NULL,
	library_id int4 NULL,
	is_paid int2 NULL,
	comment_notify_ids jsonb DEFAULT '[]'::jsonb NOT NULL,
	div_id int4 NULL,
	out_invoice_id int4 NULL,
	CONSTRAINT invoice_recieve_pkey PRIMARY KEY (invoice_recieve_id),
	CONSTRAINT invoice_receive_locked_id FOREIGN KEY (locked_id) REFERENCES public."user"(user_id) ON DELETE RESTRICT,
	CONSTRAINT invoice_receive_user_id FOREIGN KEY (user_id) REFERENCES public."user"(user_id) ON DELETE RESTRICT,
	CONSTRAINT invoice_recieve_company_id_fkey FOREIGN KEY (company_id) REFERENCES public.company(company_id) ON DELETE RESTRICT,
	CONSTRAINT invoice_recieve_out_invoice_id FOREIGN KEY (out_invoice_id) REFERENCES public.invoice(invoice_id) ON DELETE CASCADE,
	CONSTRAINT invoice_recieve_purchase_payment_id_fkey FOREIGN KEY (purchase_payment_id) REFERENCES public.purchase_payment(purchase_payment_id) ON DELETE RESTRICT,
	CONSTRAINT invoice_recieve_supplier_id_fkey FOREIGN KEY (supplier_id) REFERENCES public.supplier(supplier_id) ON DELETE RESTRICT
);
CREATE INDEX invoice_receive_purchase_ids ON public.invoice_recieve USING btree (purchase_ids);
CREATE INDEX invoice_recieve_company_id ON public.invoice_recieve USING btree (company_id);
CREATE INDEX invoice_recieve_div_id_idx ON public.invoice_recieve USING btree (div_id);
CREATE INDEX invoice_recieve_is_paid ON public.invoice_recieve USING btree (is_paid);
CREATE INDEX invoice_recieve_out_invoice_id_idx ON public.invoice_recieve USING btree (out_invoice_id);
CREATE INDEX invoice_recieve_parent_id ON public.invoice_recieve USING btree (parent_id);
CREATE INDEX invoice_recieve_state ON public.invoice_recieve USING btree (state);
CREATE INDEX invoice_recieve_supplier_id ON public.invoice_recieve USING btree (supplier_id);


-- public.invoice_recieve_attach definition

-- Drop table

-- DROP TABLE public.invoice_recieve_attach;

CREATE TABLE public.invoice_recieve_attach (
	invoice_recieve_attach_id serial4 NOT NULL,
	invoice_recieve_id int4 NOT NULL,
	"source" varchar(100) NOT NULL,
	"comment" text NULL,
	"date" timestamptz DEFAULT now() NOT NULL,
	creator_id int4 NULL,
	CONSTRAINT invoice_recieve_attach_pkey PRIMARY KEY (invoice_recieve_attach_id),
	CONSTRAINT invoice_recieve_attach_invoice_recieve_id_fkey FOREIGN KEY (invoice_recieve_id) REFERENCES public.invoice_recieve(invoice_recieve_id) ON DELETE CASCADE
);
CREATE INDEX invoice_recieve_attach_invoice_recieve_id ON public.invoice_recieve_attach USING btree (invoice_recieve_id);
CREATE INDEX invoice_recieve_attach_source_idx ON public.invoice_recieve_attach USING btree (source);


-- public.invoice_recieve_data definition

-- Drop table

-- DROP TABLE public.invoice_recieve_data;

CREATE TABLE public.invoice_recieve_data (
	invoice_recieve_data_id serial4 NOT NULL,
	invoice_recieve_id int4 NOT NULL,
	product_id int4 NULL,
	quantity numeric(12, 4) DEFAULT 0 NOT NULL,
	price numeric(12, 4) DEFAULT 0 NOT NULL,
	price_currency_id int2 NOT NULL,
	tax_product_id int2 NULL,
	"position" int4 DEFAULT 0 NOT NULL,
	serials jsonb DEFAULT '[]'::jsonb NOT NULL,
	CONSTRAINT invoice_recieve_data_pkey PRIMARY KEY (invoice_recieve_data_id),
	CONSTRAINT invoice_recieve_data_invoice_recieve_id_fkey FOREIGN KEY (invoice_recieve_id) REFERENCES public.invoice_recieve(invoice_recieve_id) ON DELETE CASCADE
);
CREATE INDEX invoice_recieve_data_invoice_recieve_id ON public.invoice_recieve_data USING btree (invoice_recieve_id);


-- public.product definition

-- Drop table

-- DROP TABLE public.product;

CREATE TABLE public.product (
	product_id serial4 NOT NULL,
	product_default_id int4 NOT NULL,
	product_code varchar(100) NULL,
	"ref" int4 NULL,
	quantity numeric(12, 2) DEFAULT 0 NOT NULL,
	quantity_reserved numeric(12, 2) DEFAULT 0 NOT NULL,
	price1 numeric(12, 4) NULL,
	price2 numeric(12, 4) NULL,
	"data" _int4 NULL,
	CONSTRAINT product_pkey PRIMARY KEY (product_id),
	CONSTRAINT product_product_default_id_fkey FOREIGN KEY (product_default_id) REFERENCES public.product_default(product_default_id) ON DELETE RESTRICT
);
CREATE INDEX product_data ON public.product USING gin (data);
CREATE INDEX product_product_code ON public.product USING btree (product_code);
CREATE INDEX product_product_default_id ON public.product USING btree (product_default_id);
CREATE INDEX product_ref ON public.product USING btree (ref);

-- Table Triggers

create trigger product_delete before
delete
    on
    public.product for each row execute function product_delete();


-- public.product_bundle definition

-- Drop table

-- DROP TABLE public.product_bundle;

CREATE TABLE public.product_bundle (
	product_default_id int4 NOT NULL,
	product_default_id_bundle int4 NOT NULL,
	quantity numeric(12, 2) DEFAULT 0 NOT NULL,
	visible int2 DEFAULT 0 NOT NULL,
	CONSTRAINT product_bundle_pkey PRIMARY KEY (product_default_id, product_default_id_bundle),
	CONSTRAINT product_bundle_product_default_id_fkey FOREIGN KEY (product_default_id) REFERENCES public.product_default(product_default_id) ON DELETE CASCADE
);


-- public.product_check definition

-- Drop table

-- DROP TABLE public.product_check;

CREATE TABLE public.product_check (
	product_default_id int4 NOT NULL,
	product_attribute_id int4 NOT NULL,
	CONSTRAINT product_check_pkey PRIMARY KEY (product_default_id, product_attribute_id),
	CONSTRAINT product_check_product_attribute_id_fkey FOREIGN KEY (product_attribute_id) REFERENCES public.product_attribute(product_attribute_id) ON DELETE CASCADE,
	CONSTRAINT product_check_product_default_id_fkey FOREIGN KEY (product_default_id) REFERENCES public.product_default(product_default_id) ON DELETE CASCADE
);


-- public.product_connection definition

-- Drop table

-- DROP TABLE public.product_connection;

CREATE TABLE public.product_connection (
	product1 int4 NOT NULL,
	product2 int4 NOT NULL,
	"type" int2 DEFAULT 1 NOT NULL,
	CONSTRAINT product_connection_product1_fkey FOREIGN KEY (product1) REFERENCES public.product_default(product_default_id) ON DELETE CASCADE,
	CONSTRAINT product_connection_product2_fkey FOREIGN KEY (product2) REFERENCES public.product_default(product_default_id) ON DELETE CASCADE
);
CREATE INDEX product_connection_product1 ON public.product_connection USING btree (product1);
CREATE INDEX product_connection_product2 ON public.product_connection USING btree (product2);


-- public.product_data_set definition

-- Drop table

-- DROP TABLE public.product_data_set;

CREATE TABLE public.product_data_set (
	product_default_id int4 NOT NULL,
	product_attribute_id int4 NOT NULL,
	product_attribute_data_id int4 NOT NULL,
	CONSTRAINT product_data_set_pkey PRIMARY KEY (product_default_id, product_attribute_id),
	CONSTRAINT product_data_set_product_attribute_data_id_fkey FOREIGN KEY (product_attribute_data_id) REFERENCES public.product_attribute_data(product_attribute_data_id) ON DELETE CASCADE,
	CONSTRAINT product_data_set_product_attribute_id_fkey FOREIGN KEY (product_attribute_id) REFERENCES public.product_attribute(product_attribute_id) ON DELETE CASCADE,
	CONSTRAINT product_data_set_product_default_id_fkey FOREIGN KEY (product_default_id) REFERENCES public.product_default(product_default_id) ON DELETE CASCADE
);
CREATE INDEX product_data_set_product_attribute_data_id ON public.product_data_set USING btree (product_attribute_data_id);

-- Table Triggers

create trigger product_data_set_delete after
delete
    on
    public.product_data_set for each row execute function product_data_set_delete();
create trigger product_data_set_insert after
insert
    on
    public.product_data_set for each row execute function product_data_set_insert();
create trigger product_data_set_update after
update
    on
    public.product_data_set for each row
    when ((old.product_attribute_data_id is distinct
from
    new.product_attribute_data_id)) execute function product_data_set_delete();


-- public.product_decimal_set definition

-- Drop table

-- DROP TABLE public.product_decimal_set;

CREATE TABLE public.product_decimal_set (
	product_default_id int4 NOT NULL,
	product_attribute_id int4 NOT NULL,
	attribute_decimal numeric(10, 2) NOT NULL,
	CONSTRAINT product_decimal_set_pkey PRIMARY KEY (product_default_id, product_attribute_id),
	CONSTRAINT product_decimal_set_product_attribute_id_fkey FOREIGN KEY (product_attribute_id) REFERENCES public.product_attribute(product_attribute_id) ON DELETE CASCADE,
	CONSTRAINT product_decimal_set_product_default_id_fkey FOREIGN KEY (product_default_id) REFERENCES public.product_default(product_default_id) ON DELETE CASCADE
);
CREATE INDEX product_decimal_set_attribute_decimal ON public.product_decimal_set USING btree (attribute_decimal);


-- public.product_dictionary definition

-- Drop table

-- DROP TABLE public.product_dictionary;

CREATE TABLE public.product_dictionary (
	did bigserial NOT NULL,
	hash bytea NULL,
	product_id int4 NOT NULL,
	CONSTRAINT product_dictionary_pkey PRIMARY KEY (did),
	CONSTRAINT product_dictionary_product_id_fkey FOREIGN KEY (product_id) REFERENCES public.product(product_id)
);
CREATE INDEX product_dictionary_hash_idx ON public.product_dictionary USING btree (hash);
CREATE INDEX product_dictionary_product_id_idx ON public.product_dictionary USING btree (product_id);


-- public.product_reserved definition

-- Drop table

-- DROP TABLE public.product_reserved;

CREATE TABLE public.product_reserved (
	product_reserved_id varchar(100) NOT NULL,
	reason varchar(100) NOT NULL,
	product_id int4 NOT NULL,
	quantity numeric(12, 2) NOT NULL,
	CONSTRAINT product_reserved_pkey PRIMARY KEY (product_reserved_id),
	CONSTRAINT product_reserved_product_id_fkey FOREIGN KEY (product_id) REFERENCES public.product(product_id) ON DELETE CASCADE
);


-- public.product_variant_map definition

-- Drop table

-- DROP TABLE public.product_variant_map;

CREATE TABLE public.product_variant_map (
	product_variant_map_id serial4 NOT NULL,
	product_id int4 NOT NULL,
	product_attribute_data_id int4 NOT NULL,
	CONSTRAINT product_variant_map_pkey PRIMARY KEY (product_variant_map_id),
	CONSTRAINT product_variant_map_product_attribute_data_id_fkey FOREIGN KEY (product_attribute_data_id) REFERENCES public.product_attribute_data(product_attribute_data_id) ON DELETE RESTRICT,
	CONSTRAINT product_variant_map_product_id_fkey FOREIGN KEY (product_id) REFERENCES public.product(product_id) ON DELETE CASCADE
);
CREATE INDEX product_variant_map_product_attribute_data_id ON public.product_variant_map USING btree (product_attribute_data_id);
CREATE INDEX product_variant_map_product_id ON public.product_variant_map USING btree (product_id);

-- Table Triggers

create trigger product_variant_map_delete after
delete
    on
    public.product_variant_map for each row execute function product_variant_map_delete();
create trigger product_variant_map_insert after
insert
    on
    public.product_variant_map for each row execute function product_variant_map_insert();
create trigger product_variant_map_update after
update
    on
    public.product_variant_map for each row
    when ((old.product_attribute_data_id is distinct
from
    new.product_attribute_data_id)) execute function product_variant_map_delete();


-- public.sale_attach definition

-- Drop table

-- DROP TABLE public.sale_attach;

CREATE TABLE public.sale_attach (
	sale_attach_id serial4 NOT NULL,
	sale_id int4 NULL,
	"source" varchar(100) NOT NULL,
	"comment" text NULL,
	creator_id int4 NULL,
	"date" timestamptz NULL,
	req_id int8 NULL,
	CONSTRAINT sale_attach_pkey PRIMARY KEY (sale_attach_id),
	CONSTRAINT sale_attach_req_id_fkey FOREIGN KEY (req_id) REFERENCES public.sl_req_offer(req_id) ON DELETE CASCADE,
	CONSTRAINT sale_attach_sale_id_fkey FOREIGN KEY (sale_id) REFERENCES public.sale(sale_id) ON DELETE CASCADE
);
CREATE INDEX sale_attach_req_id ON public.sale_attach USING btree (req_id);
CREATE INDEX sale_attach_sale_id ON public.sale_attach USING btree (sale_id);
CREATE INDEX sale_attach_source ON public.sale_attach USING btree (source);


-- public.sale_cancelled_cash_data definition

-- Drop table

-- DROP TABLE public.sale_cancelled_cash_data;

CREATE TABLE public.sale_cancelled_cash_data (
	data_id serial4 NOT NULL,
	sale_payment_id int4 NOT NULL,
	"timestamp" timestamptz NULL,
	operator_id int4 NOT NULL,
	unp varchar(80) NULL,
	"method" int4 DEFAULT 1 NOT NULL,
	printer_id int4 NULL,
	is_invoice bool DEFAULT false NOT NULL,
	CONSTRAINT sale_cancelled_cash_data_pkey PRIMARY KEY (data_id),
	CONSTRAINT sale_cancelled_cash_data_operator_id_fkey FOREIGN KEY (operator_id) REFERENCES public.sale_cash_operator(operator_id) ON DELETE CASCADE,
	CONSTRAINT sale_cancelled_cash_data_sale_payment_id_fkey FOREIGN KEY (sale_payment_id) REFERENCES public.sale_payment(sale_payment_id) ON DELETE RESTRICT
);


-- public.sale_cash_data definition

-- Drop table

-- DROP TABLE public.sale_cash_data;

CREATE TABLE public.sale_cash_data (
	sale_payment_id int4 NOT NULL,
	"timestamp" timestamptz NULL,
	created timestamptz DEFAULT now() NOT NULL,
	unp varchar(80) NULL,
	"method" int4 DEFAULT 1 NOT NULL,
	fiscal_data jsonb NULL,
	cash_lines jsonb DEFAULT '[]'::jsonb NOT NULL,
	"number" int4 NULL,
	printer_id int4 NULL,
	"comment" text NULL,
	operator_id int4 NOT NULL,
	reversed bool DEFAULT false NOT NULL,
	is_invoice bool DEFAULT false NOT NULL,
	fiscal_number int4 NULL,
	CONSTRAINT sale_cash_data_pkey PRIMARY KEY (sale_payment_id),
	CONSTRAINT sale_cash_data_unp_key UNIQUE (unp),
	CONSTRAINT sale_cash_data_operator_id_fkey FOREIGN KEY (operator_id) REFERENCES public.sale_cash_operator(operator_id) ON DELETE CASCADE,
	CONSTRAINT sale_cash_data_sale_payment_id_fkey FOREIGN KEY (sale_payment_id) REFERENCES public.sale_payment(sale_payment_id) ON DELETE CASCADE
);
CREATE INDEX sale_cancelled_cash_data_sale_payment_id ON public.sale_cash_data USING btree (sale_payment_id);
CREATE INDEX sale_cash_data_fiscal_number ON public.sale_cash_data USING btree (fiscal_number);
CREATE INDEX sale_cash_data_payment ON public.sale_cash_data USING btree (sale_payment_id);


-- public.sale_hand_over_line definition

-- Drop table

-- DROP TABLE public.sale_hand_over_line;

CREATE TABLE public.sale_hand_over_line (
	line_id serial4 NOT NULL,
	ho_id int4 NOT NULL,
	product_id int4 NOT NULL,
	quantity numeric(12, 2) DEFAULT 0 NOT NULL,
	price numeric(12, 2) DEFAULT 0 NOT NULL,
	price_currency_id int4 DEFAULT 1 NOT NULL,
	tax_product_id int4 NOT NULL,
	"position" int4 DEFAULT 1 NOT NULL,
	CONSTRAINT sale_hand_over_line_pkey PRIMARY KEY (line_id),
	CONSTRAINT sale_hand_over_line_ho_id_fkey FOREIGN KEY (ho_id) REFERENCES public.sale_hand_over(ho_id) ON DELETE CASCADE,
	CONSTRAINT sale_hand_over_line_product_id FOREIGN KEY (product_id) REFERENCES public.product(product_id) ON DELETE RESTRICT
);
CREATE INDEX sale_hand_over_line_ho_id_idx ON public.sale_hand_over_line USING btree (ho_id);
CREATE INDEX sale_hand_over_line_position_idx ON public.sale_hand_over_line USING btree ("position");
CREATE INDEX sale_hand_over_line_product_id_idx ON public.sale_hand_over_line USING btree (product_id);


-- public.sale_invoice_state definition

-- Drop table

-- DROP TABLE public.sale_invoice_state;

CREATE TABLE public.sale_invoice_state (
	invoice_id int4 NOT NULL,
	state bool DEFAULT true NOT NULL,
	CONSTRAINT sale_invoice_state_pkey PRIMARY KEY (invoice_id),
	CONSTRAINT sale_invoice_state_invoice_id_fkey FOREIGN KEY (invoice_id) REFERENCES public.invoice(invoice_id) ON DELETE CASCADE
);
CREATE INDEX sale_invoice_state_state_idx ON public.sale_invoice_state USING btree (state);


-- public.sale_unpaid_rec definition

-- Drop table

-- DROP TABLE public.sale_unpaid_rec;

CREATE TABLE public.sale_unpaid_rec (
	invoice_id int4 NOT NULL,
	message text NULL,
	es_id int8 NOT NULL,
	created timestamptz DEFAULT now() NOT NULL,
	CONSTRAINT sale_unpaid_rec_pkey PRIMARY KEY (invoice_id),
	CONSTRAINT sale_unpaid_rec_invoice_id_fkey FOREIGN KEY (invoice_id) REFERENCES public.invoice(invoice_id)
);
CREATE INDEX sale_unpaid_rec_created_idx ON public.sale_unpaid_rec USING btree (created);
CREATE INDEX sale_unpaid_rec_invoice_id_idx ON public.sale_unpaid_rec USING btree (invoice_id);


-- public.sl_req_import_data definition

-- Drop table

-- DROP TABLE public.sl_req_import_data;

CREATE TABLE public.sl_req_import_data (
	data_id bigserial NOT NULL,
	ri_id int8 NOT NULL,
	req_id int8 NULL,
	product_id int4 NULL,
	quantity numeric(12, 4) DEFAULT 0 NOT NULL,
	"name" text NULL,
	"position" int4 DEFAULT 0 NOT NULL,
	CONSTRAINT sl_req_import_data_pkey PRIMARY KEY (data_id),
	CONSTRAINT sl_req_import_data_req_id FOREIGN KEY (req_id) REFERENCES public.sl_req_offer(req_id) ON DELETE SET NULL,
	CONSTRAINT sl_req_import_data_req_id_fkey FOREIGN KEY (req_id) REFERENCES public.sl_req_offer(req_id) ON DELETE CASCADE,
	CONSTRAINT sl_req_import_data_ri_id_fkey FOREIGN KEY (ri_id) REFERENCES public.sl_req_import(ri_id) ON DELETE CASCADE
);
CREATE INDEX sl_req_import_data_position_idx ON public.sl_req_import_data USING btree ("position");
CREATE INDEX sl_req_import_data_req_id_idx ON public.sl_req_import_data USING btree (req_id);
CREATE INDEX sl_req_import_data_ri_id_idx ON public.sl_req_import_data USING btree (ri_id);


-- public.support_answer definition

-- Drop table

-- DROP TABLE public.support_answer;

CREATE TABLE public.support_answer (
	answer_id serial4 NOT NULL,
	cat_id int4 NULL,
	type_id int4 NULL,
	title text NULL,
	descr text NULL,
	request_id int4 NULL,
	user_id int4 NOT NULL,
	answer_stamp timestamptz DEFAULT now() NOT NULL,
	received bool DEFAULT false NOT NULL,
	CONSTRAINT support_answer_pkey PRIMARY KEY (answer_id),
	CONSTRAINT support_answer_cat_id_fkey FOREIGN KEY (cat_id) REFERENCES public.support_cat(cat_id) ON DELETE SET NULL,
	CONSTRAINT support_answer_request_id_fkey FOREIGN KEY (request_id) REFERENCES public.support_request(request_id) ON DELETE CASCADE,
	CONSTRAINT support_answer_type_id_fkey FOREIGN KEY (type_id) REFERENCES public.support_type(type_id) ON DELETE SET NULL
);
CREATE INDEX support_answer_answer_stamp_idx ON public.support_answer USING btree (answer_stamp);
CREATE INDEX support_answer_cat_id_idx ON public.support_answer USING btree (cat_id);
CREATE INDEX support_answer_received_idx ON public.support_answer USING btree (received);
CREATE INDEX support_answer_request_id_idx ON public.support_answer USING btree (request_id);
CREATE INDEX support_answer_title_idx ON public.support_answer USING btree (title);
CREATE INDEX support_answer_type_id_idx ON public.support_answer USING btree (type_id);
CREATE INDEX support_answer_user_id_idx ON public.support_answer USING btree (user_id);


-- public.support_chat_process definition

-- Drop table

-- DROP TABLE public.support_chat_process;

CREATE TABLE public.support_chat_process (
	cp_id bigserial NOT NULL,
	request_id int4 NOT NULL,
	customer_id int4 NOT NULL,
	approve_id int4 NULL,
	req_stamp timestamptz DEFAULT now() NOT NULL,
	approve_stamp timestamptz NULL,
	close_stamp timestamptz NULL,
	client_name varchar(120) NULL,
	member_ids jsonb DEFAULT '[]'::jsonb NOT NULL,
	CONSTRAINT support_chat_process_pkey PRIMARY KEY (cp_id),
	CONSTRAINT support_chat_process_request_id_fkey FOREIGN KEY (request_id) REFERENCES public.support_request(request_id)
);
CREATE INDEX support_chat_process_approve_id_idx ON public.support_chat_process USING btree (approve_id);
CREATE INDEX support_chat_process_approve_stamp_idx ON public.support_chat_process USING btree (approve_stamp);
CREATE INDEX support_chat_process_close_stamp_idx ON public.support_chat_process USING btree (close_stamp);
CREATE INDEX support_chat_process_customer_id_idx ON public.support_chat_process USING btree (customer_id);
CREATE INDEX support_chat_process_member_ids_idx ON public.support_chat_process USING btree (member_ids);
CREATE INDEX support_chat_process_req_stamp_idx ON public.support_chat_process USING btree (req_stamp);
CREATE INDEX support_chat_process_request_id_idx ON public.support_chat_process USING btree (request_id);


-- public.support_comments definition

-- Drop table

-- DROP TABLE public.support_comments;

CREATE TABLE public.support_comments (
	comment_id serial4 NOT NULL,
	request_id int4 NULL,
	created timestamptz NOT NULL,
	creator_id int4 DEFAULT 1 NOT NULL,
	"comment" text NULL,
	address_id jsonb DEFAULT '[]'::jsonb NOT NULL,
	CONSTRAINT support_comments_pkey PRIMARY KEY (comment_id),
	CONSTRAINT support_comments_request_id_fkey FOREIGN KEY (request_id) REFERENCES public.support_request(request_id) ON DELETE CASCADE
);
CREATE INDEX support_comments_created_idx ON public.support_comments USING btree (created);
CREATE INDEX support_comments_request_id_idx ON public.support_comments USING btree (request_id);


-- public.support_req_timer definition

-- Drop table

-- DROP TABLE public.support_req_timer;

CREATE TABLE public.support_req_timer (
	request_id int4 NOT NULL,
	approved timestamptz DEFAULT now() NOT NULL,
	deadline timestamptz NOT NULL,
	event_id int4 NULL,
	CONSTRAINT support_req_timer_request_id_fkey FOREIGN KEY (request_id) REFERENCES public.support_request(request_id) ON DELETE CASCADE
);
CREATE INDEX support_req_timer_approved_idx ON public.support_req_timer USING btree (approved);
CREATE INDEX support_req_timer_deadline_idx ON public.support_req_timer USING btree (deadline);
CREATE INDEX support_req_timer_request_id_idx ON public.support_req_timer USING btree (request_id);


-- public.warehouse_inventory definition

-- Drop table

-- DROP TABLE public.warehouse_inventory;

CREATE TABLE public.warehouse_inventory (
	warehouse_inventory_id serial4 NOT NULL,
	product_id int4 NOT NULL,
	warehouse_id int4 NOT NULL,
	quantity numeric(12, 2) DEFAULT 0 NOT NULL,
	quantity_min numeric(12, 2) NULL,
	CONSTRAINT warehouse_inventory_pkey PRIMARY KEY (warehouse_inventory_id),
	CONSTRAINT warehouse_inventory_product_id_warehouse_id_key UNIQUE (product_id, warehouse_id),
	CONSTRAINT warehouse_inventory_product_id_fkey FOREIGN KEY (product_id) REFERENCES public.product(product_id) ON DELETE CASCADE,
	CONSTRAINT warehouse_inventory_warehouse_id_fkey FOREIGN KEY (warehouse_id) REFERENCES public.warehouse(warehouse_id) ON DELETE RESTRICT
);
CREATE INDEX warehouse_inventory_product_id ON public.warehouse_inventory USING btree (product_id);
CREATE INDEX warehouse_inventory_warehouse_id ON public.warehouse_inventory USING btree (warehouse_id);


-- public.warehouse_inventory_serial definition

-- Drop table

-- DROP TABLE public.warehouse_inventory_serial;

CREATE TABLE public.warehouse_inventory_serial (
	warehouse_inventory_serial_id serial4 NOT NULL,
	warehouse_inventory_id int4 NOT NULL,
	serial varchar(100) NULL,
	quantity int4 DEFAULT 0 NOT NULL,
	"time" timestamptz DEFAULT now() NOT NULL,
	purchase_id int4 NULL,
	sale_id int4 NULL,
	CONSTRAINT warehouse_inventory_serial_pkey PRIMARY KEY (warehouse_inventory_serial_id),
	CONSTRAINT warehouse_inventory_serial_warehouse_inventory_id_serial_key UNIQUE (warehouse_inventory_id, serial),
	CONSTRAINT warehouse_inventory_serial_warehouse_inventory_id_fkey FOREIGN KEY (warehouse_inventory_id) REFERENCES public.warehouse_inventory(warehouse_inventory_id) ON DELETE CASCADE
);
CREATE INDEX warehouse_inventory_serial_purchase_id_idx ON public.warehouse_inventory_serial USING btree (purchase_id);
CREATE INDEX warehouse_inventory_serial_sale_id_idx ON public.warehouse_inventory_serial USING btree (sale_id);
CREATE INDEX warehouse_inventory_serial_serial_idx ON public.warehouse_inventory_serial USING btree (serial);
CREATE INDEX warehouse_inventory_serial_time_idx ON public.warehouse_inventory_serial USING btree ("time");
CREATE INDEX warehouse_inventory_serial_warehouse_inventory_id_idx ON public.warehouse_inventory_serial USING btree (warehouse_inventory_id);


-- public.case_address definition

-- Drop table

-- DROP TABLE public.case_address;

CREATE TABLE public.case_address (
	address_id serial4 NOT NULL,
	country_id int4 NOT NULL,
	country_zone_id int4 NOT NULL,
	mun_id int4 NULL,
	town_id int4 NULL,
	qtr_id int4 NULL,
	cplx_id int4 NULL,
	str_id int4 NULL,
	numb varchar(10) NULL,
	blck varchar(30) NULL,
	entrance varchar(10) NULL,
	floor int2 NULL,
	post int4 NULL,
	appartment varchar(10) NULL,
	address text NULL,
	lat float4 NULL,
	lng float4 NULL,
	CONSTRAINT case_address_pkey PRIMARY KEY (address_id),
	CONSTRAINT case_address_cplx_id_fkey FOREIGN KEY (cplx_id) REFERENCES public.case_complex(cplx_id) ON DELETE RESTRICT,
	CONSTRAINT case_address_mun_id_fkey FOREIGN KEY (mun_id) REFERENCES public.case_municipality(mun_id) ON DELETE RESTRICT,
	CONSTRAINT case_address_qtr_id_fkey FOREIGN KEY (qtr_id) REFERENCES public.case_quarter(qtr_id) ON DELETE RESTRICT,
	CONSTRAINT case_address_str_id_fkey FOREIGN KEY (str_id) REFERENCES public.case_street(str_id) ON DELETE RESTRICT,
	CONSTRAINT case_address_town_id_fkey FOREIGN KEY (town_id) REFERENCES public.case_town(town_id) ON DELETE RESTRICT
);
CREATE INDEX case_address_country_id_idx ON public.case_address USING btree (country_id);
CREATE INDEX case_address_country_zone_id_idx ON public.case_address USING btree (country_zone_id);
CREATE INDEX case_address_cplx_id_idx ON public.case_address USING btree (cplx_id);
CREATE INDEX case_address_mun_id_idx ON public.case_address USING btree (mun_id);
CREATE INDEX case_address_post_idx ON public.case_address USING btree (post);
CREATE INDEX case_address_qtr_id_idx ON public.case_address USING btree (qtr_id);
CREATE INDEX case_address_str_id_idx ON public.case_address USING btree (str_id);
CREATE INDEX case_address_town_id_idx ON public.case_address USING btree (town_id);


-- public.case_invoice definition

-- Drop table

-- DROP TABLE public.case_invoice;

CREATE TABLE public.case_invoice (
	link_id serial4 NOT NULL,
	case_id int4 NOT NULL,
	invoice_id int4 NOT NULL,
	stamp timestamptz DEFAULT now() NOT NULL,
	CONSTRAINT case_invoice_pkey PRIMARY KEY (link_id),
	CONSTRAINT case_invoice_case_id_fkey FOREIGN KEY (case_id) REFERENCES public.case_case(case_id) ON DELETE CASCADE,
	CONSTRAINT case_invoice_invoice_id FOREIGN KEY (invoice_id) REFERENCES public.invoice(invoice_id) ON DELETE CASCADE
);
CREATE INDEX case_invoice_case_id_idx ON public.case_invoice USING btree (case_id);
CREATE INDEX case_invoice_invoice_id_idx ON public.case_invoice USING btree (invoice_id);
CREATE INDEX case_invoice_stamp_idx ON public.case_invoice USING btree (stamp);


-- public.cases_opis_sched definition

-- Drop table

-- DROP TABLE public.cases_opis_sched;

CREATE TABLE public.cases_opis_sched (
	op_id serial4 NOT NULL,
	stamp timestamptz DEFAULT now() NOT NULL,
	case_id int4 NOT NULL,
	debtor_id int4 NOT NULL,
	address_id int4 NOT NULL,
	prop_id int4 NOT NULL,
	done bool DEFAULT false NOT NULL,
	created timestamptz DEFAULT now() NOT NULL,
	CONSTRAINT cases_opis_sched_pkey PRIMARY KEY (op_id),
	CONSTRAINT cases_opis_sched_address_id_fkey FOREIGN KEY (address_id) REFERENCES public.case_address(address_id) ON DELETE RESTRICT,
	CONSTRAINT cases_opis_sched_case_id_fkey FOREIGN KEY (case_id) REFERENCES public.case_case(case_id) ON DELETE CASCADE,
	CONSTRAINT cases_opis_sched_debtor_id_fkey FOREIGN KEY (debtor_id) REFERENCES public.case_person(person_id) ON DELETE RESTRICT
);
CREATE INDEX cases_opis_sched_address_id_idx ON public.cases_opis_sched USING btree (address_id);
CREATE INDEX cases_opis_sched_case_id_idx ON public.cases_opis_sched USING btree (case_id);
CREATE INDEX cases_opis_sched_created_idx ON public.cases_opis_sched USING btree (created);
CREATE INDEX cases_opis_sched_debtor_id_idx ON public.cases_opis_sched USING btree (debtor_id);
CREATE INDEX cases_opis_sched_done_idx ON public.cases_opis_sched USING btree (done);
CREATE INDEX cases_opis_sched_stamp_idx ON public.cases_opis_sched USING btree (stamp);


-- public.cl_purchase_comments definition

-- Drop table

-- DROP TABLE public.cl_purchase_comments;

CREATE TABLE public.cl_purchase_comments (
	comment_id serial4 NOT NULL,
	purchase_id int4 NULL,
	invoice_id int4 NULL,
	created timestamptz NOT NULL,
	creator_id int4 DEFAULT 1 NOT NULL,
	address_id jsonb DEFAULT '[]'::jsonb NOT NULL,
	"comment" text NULL,
	CONSTRAINT cl_purchase_comments_pkey PRIMARY KEY (comment_id),
	CONSTRAINT cl_purchase_comments_invoice_id_fkey FOREIGN KEY (invoice_id) REFERENCES public.invoice_recieve(invoice_recieve_id),
	CONSTRAINT cl_purchase_comments_purchase_id_fkey FOREIGN KEY (purchase_id) REFERENCES public.purchase(purchase_id)
);
CREATE INDEX cl_purchase_comments_invoice_id ON public.cl_purchase_comments USING btree (invoice_id);
CREATE INDEX cl_purchase_comments_purchase_id ON public.cl_purchase_comments USING btree (purchase_id);


-- public.cl_sale_comments definition

-- Drop table

-- DROP TABLE public.cl_sale_comments;

CREATE TABLE public.cl_sale_comments (
	comment_id serial4 NOT NULL,
	sale_id int4 NULL,
	invoice_id int4 NULL,
	created timestamptz NOT NULL,
	creator_id int4 DEFAULT 1 NOT NULL,
	"comment" text NULL,
	address_id jsonb DEFAULT '[]'::jsonb NOT NULL,
	req_id int8 NULL,
	CONSTRAINT cl_sale_comments_pkey PRIMARY KEY (comment_id),
	CONSTRAINT cl_sale_comments_invoice_id_fkey FOREIGN KEY (invoice_id) REFERENCES public.invoice(invoice_id) ON DELETE CASCADE,
	CONSTRAINT cl_sale_comments_req_id_fkey FOREIGN KEY (req_id) REFERENCES public.sl_req_offer(req_id) ON DELETE CASCADE,
	CONSTRAINT cl_sale_comments_sale_id_fkey FOREIGN KEY (sale_id) REFERENCES public.sale(sale_id) ON DELETE CASCADE
);
CREATE INDEX cl_sale_comments_invoice_id ON public.cl_sale_comments USING btree (invoice_id);
CREATE INDEX cl_sale_comments_req_id_idx ON public.cl_sale_comments USING btree (req_id);
CREATE INDEX cl_sale_comments_sale_id ON public.cl_sale_comments USING btree (sale_id);


-- public.case_account definition

-- Drop table

-- DROP TABLE public.case_account;

CREATE TABLE public.case_account (
	acc_id serial4 NOT NULL,
	case_id int4 NOT NULL,
	debtor_id int4 NOT NULL,
	address_id int4 NULL,
	stamp timestamptz DEFAULT now() NOT NULL,
	render_doc text NULL,
	acc_nr int8 NULL,
	seq_id int2 DEFAULT 0 NOT NULL,
	invoice_id int4 NULL,
	CONSTRAINT case_account_acc_nr_seq_id_key UNIQUE (acc_nr, seq_id),
	CONSTRAINT case_account_pkey PRIMARY KEY (acc_id),
	CONSTRAINT case_account_address_id_fkey FOREIGN KEY (address_id) REFERENCES public.case_address(address_id) ON DELETE RESTRICT,
	CONSTRAINT case_account_case_id_fkey FOREIGN KEY (case_id) REFERENCES public.case_case(case_id) ON DELETE CASCADE,
	CONSTRAINT case_account_debtor_id_fkey FOREIGN KEY (debtor_id) REFERENCES public.case_person(person_id) ON DELETE RESTRICT
);
CREATE INDEX case_account_acc_nr_idx ON public.case_account USING btree (acc_nr);
CREATE INDEX case_account_address_id_idx ON public.case_account USING btree (address_id);
CREATE INDEX case_account_case_id_idx ON public.case_account USING btree (case_id);
CREATE INDEX case_account_debtor_id_idx ON public.case_account USING btree (debtor_id);
CREATE INDEX case_account_invoice_id_idx ON public.case_account USING btree (invoice_id);
CREATE INDEX case_account_seq_id_idx ON public.case_account USING btree (seq_id);
CREATE INDEX case_account_stamp_idx ON public.case_account USING btree (stamp);


-- public.case_addr_data definition

-- Drop table

-- DROP TABLE public.case_addr_data;

CREATE TABLE public.case_addr_data (
	ad_id serial4 NOT NULL,
	address_id int4 NOT NULL,
	created timestamptz DEFAULT now() NOT NULL,
	modified timestamptz DEFAULT now() NOT NULL,
	created_id int4 NOT NULL,
	modified_id int4 NOT NULL,
	"role" int2 NOT NULL,
	active bool DEFAULT true NOT NULL,
	person_id int4 NULL,
	is_default bool DEFAULT false NOT NULL,
	CONSTRAINT case_addr_data_pkey PRIMARY KEY (ad_id),
	CONSTRAINT case_addr_data_address_id FOREIGN KEY (address_id) REFERENCES public.case_address(address_id) ON DELETE RESTRICT,
	CONSTRAINT case_addr_data_person_id_fkey FOREIGN KEY (person_id) REFERENCES public.case_person(person_id) ON DELETE RESTRICT
);
CREATE INDEX case_addr_data_active_idx ON public.case_addr_data USING btree (active);
CREATE INDEX case_addr_data_address_id_idx ON public.case_addr_data USING btree (address_id);
CREATE INDEX case_addr_data_created_id_idx ON public.case_addr_data USING btree (created_id);
CREATE INDEX case_addr_data_created_idx ON public.case_addr_data USING btree (created);
CREATE INDEX case_addr_data_is_default_idx ON public.case_addr_data USING btree (is_default);
CREATE INDEX case_addr_data_modified_id_idx ON public.case_addr_data USING btree (modified_id);
CREATE INDEX case_addr_data_modified_idx ON public.case_addr_data USING btree (modified);
CREATE INDEX case_addr_data_person_id_idx ON public.case_addr_data USING btree (person_id);
CREATE INDEX case_addr_data_role_idx ON public.case_addr_data USING btree (role);


-- public.case_bank_tax definition

-- Drop table

-- DROP TABLE public.case_bank_tax;

CREATE TABLE public.case_bank_tax (
	tax_id serial4 NOT NULL,
	case_id int4 NOT NULL,
	stamp timestamptz DEFAULT now() NOT NULL,
	debtor_id int4 NOT NULL,
	amount numeric(12, 3) DEFAULT 0 NOT NULL,
	currency_id int4 DEFAULT 1 NOT NULL,
	"comment" text NULL,
	account_id int8 NULL,
	CONSTRAINT case_bank_tax_pkey PRIMARY KEY (tax_id),
	CONSTRAINT case_bank_tax_account_id_fkey FOREIGN KEY (account_id) REFERENCES public.case_account(acc_id),
	CONSTRAINT case_bank_tax_case_id_fkey FOREIGN KEY (case_id) REFERENCES public.case_case(case_id) ON DELETE CASCADE,
	CONSTRAINT case_bank_tax_debtor_id_fkey FOREIGN KEY (debtor_id) REFERENCES public.case_person(person_id) ON DELETE RESTRICT
);
CREATE INDEX case_bank_tax_account_id_idx ON public.case_bank_tax USING btree (account_id);
CREATE INDEX case_bank_tax_case_id_idx ON public.case_bank_tax USING btree (case_id);
CREATE INDEX case_bank_tax_debtor_id_idx ON public.case_bank_tax USING btree (debtor_id);
CREATE INDEX case_bank_tax_stamp_idx ON public.case_bank_tax USING btree (stamp);


-- public.case_payment definition

-- Drop table

-- DROP TABLE public.case_payment;

CREATE TABLE public.case_payment (
	pay_id serial4 NOT NULL,
	person_id int4 NOT NULL,
	stamp timestamptz DEFAULT now() NOT NULL,
	source_id int2 DEFAULT 0 NOT NULL,
	case_id int4 NOT NULL,
	ref_id int4 NOT NULL,
	"role" int2 DEFAULT 1 NOT NULL,
	incoming bool DEFAULT true NOT NULL,
	pay_for int2 DEFAULT 0 NOT NULL,
	amount numeric(12, 3) DEFAULT 0 NOT NULL,
	currency_id int4 DEFAULT 1 NOT NULL,
	target int2 DEFAULT 1 NOT NULL,
	type_id int2 DEFAULT 1::smallint NOT NULL,
	"comment" text NULL,
	arr_id int4 NULL,
	out_money_id int4 NULL,
	user_id int4 NULL,
	invoice_id int8 NULL,
	account_id int8 NULL,
	old_invoice text NULL,
	CONSTRAINT case_payment_pkey PRIMARY KEY (pay_id),
	CONSTRAINT case_payment_account_id_fkey FOREIGN KEY (account_id) REFERENCES public.case_account(acc_id),
	CONSTRAINT case_payment_arr_id_fkey FOREIGN KEY (arr_id) REFERENCES public.case_arrived_money(arr_id) ON DELETE RESTRICT,
	CONSTRAINT case_payment_case_id_fkey FOREIGN KEY (case_id) REFERENCES public.case_case(case_id) ON DELETE RESTRICT,
	CONSTRAINT case_payment_out_money_id_fkey FOREIGN KEY (out_money_id) REFERENCES public.case_out_money(money_id) ON DELETE RESTRICT,
	CONSTRAINT case_payment_person_id_fkey FOREIGN KEY (person_id) REFERENCES public.case_person(person_id) ON DELETE RESTRICT,
	CONSTRAINT case_payment_user_id FOREIGN KEY (user_id) REFERENCES public."user"(user_id) ON DELETE RESTRICT
);
CREATE INDEX case_payment_account_id_idx ON public.case_payment USING btree (account_id);
CREATE INDEX case_payment_amount_idx ON public.case_payment USING btree (amount);
CREATE INDEX case_payment_arr_id_idx ON public.case_payment USING btree (arr_id);
CREATE INDEX case_payment_case_id_idx ON public.case_payment USING btree (case_id);
CREATE INDEX case_payment_currency_id_idx ON public.case_payment USING btree (currency_id);
CREATE INDEX case_payment_incoming_idx ON public.case_payment USING btree (incoming);
CREATE INDEX case_payment_invoice_id_idx ON public.case_payment USING btree (invoice_id);
CREATE INDEX case_payment_old_invoice_idx ON public.case_payment USING btree (old_invoice);
CREATE INDEX case_payment_out_money_id_idx ON public.case_payment USING btree (out_money_id);
CREATE INDEX case_payment_pay_for_idx ON public.case_payment USING btree (pay_for);
CREATE INDEX case_payment_person_id_idx ON public.case_payment USING btree (person_id);
CREATE INDEX case_payment_ref_id_idx ON public.case_payment USING btree (ref_id);
CREATE INDEX case_payment_role_idx ON public.case_payment USING btree (role);
CREATE INDEX case_payment_source_id_idx ON public.case_payment USING btree (source_id);
CREATE INDEX case_payment_stamp_idx ON public.case_payment USING btree (stamp);
CREATE INDEX case_payment_target_idx ON public.case_payment USING btree (target);
CREATE INDEX case_payment_type_id_idx ON public.case_payment USING btree (type_id);
CREATE INDEX case_payment_user_id_idx ON public.case_payment USING btree (user_id);


-- public.case_attach definition

-- Drop table

-- DROP TABLE public.case_attach;

CREATE TABLE public.case_attach (
	att_id serial4 NOT NULL,
	case_id int4 NULL,
	creator_id int4 NOT NULL,
	type_id int4 DEFAULT 0 NOT NULL,
	stype_id int4 DEFAULT 0 NOT NULL,
	"date" timestamptz DEFAULT now() NOT NULL,
	"source" varchar(120) NULL,
	doc_number int4 DEFAULT 0 NOT NULL,
	"comment" text NULL,
	connected_ids jsonb DEFAULT '[]'::jsonb NOT NULL,
	person_id int4 NULL,
	debtor_id int4 NULL,
	creditor_id int4 NULL,
	sstype_id int4 DEFAULT 0 NOT NULL,
	out_doc_id int4 NULL,
	parent_id int8 NULL,
	addon_flags int8 DEFAULT 0 NOT NULL,
	CONSTRAINT case_attach_pkey PRIMARY KEY (att_id)
);
CREATE INDEX case_attach_addon_flags_idx ON public.case_attach USING btree (addon_flags);
CREATE INDEX case_attach_case_id_idx ON public.case_attach USING btree (case_id);
CREATE INDEX case_attach_connected_ids_idx ON public.case_attach USING btree (connected_ids);
CREATE INDEX case_attach_creator_id_idx ON public.case_attach USING btree (creator_id);
CREATE INDEX case_attach_creditor_id_idx ON public.case_attach USING btree (creditor_id);
CREATE INDEX case_attach_date_idx ON public.case_attach USING btree (date);
CREATE INDEX case_attach_debtor_id_idx ON public.case_attach USING btree (debtor_id);
CREATE INDEX case_attach_doc_number_idx ON public.case_attach USING btree (doc_number);
CREATE INDEX case_attach_out_doc_id_idx ON public.case_attach USING btree (out_doc_id);
CREATE INDEX case_attach_person_id_idx ON public.case_attach USING btree (person_id);
CREATE INDEX case_attach_source_idx ON public.case_attach USING btree (source);
CREATE INDEX case_attach_sstype_id_idx ON public.case_attach USING btree (sstype_id);
CREATE INDEX case_attach_stype_id_idx ON public.case_attach USING btree (stype_id);
CREATE INDEX case_attach_type_id_idx ON public.case_attach USING btree (type_id);


-- public.case_in_doc_tax definition

-- Drop table

-- DROP TABLE public.case_in_doc_tax;

CREATE TABLE public.case_in_doc_tax (
	tax_id serial4 NOT NULL,
	att_id int4 NOT NULL,
	"type" int2 DEFAULT 0 NOT NULL,
	price numeric(12, 3) DEFAULT 0 NOT NULL,
	price31 numeric(12, 3) DEFAULT 0 NOT NULL,
	deed bool DEFAULT true NOT NULL,
	invoice_id int8 NULL,
	account_id int8 NULL,
	stamp timestamptz DEFAULT now() NOT NULL,
	add_to_debt bool DEFAULT true NOT NULL,
	CONSTRAINT case_in_doc_tax_pkey PRIMARY KEY (tax_id)
);
CREATE INDEX case_in_doc_tax_account_id_idx ON public.case_in_doc_tax USING btree (account_id);
CREATE INDEX case_in_doc_tax_add_to_debt_idx ON public.case_in_doc_tax USING btree (add_to_debt);
CREATE INDEX case_in_doc_tax_att_id_idx ON public.case_in_doc_tax USING btree (att_id);
CREATE INDEX case_in_doc_tax_deed_idx ON public.case_in_doc_tax USING btree (deed);
CREATE INDEX case_in_doc_tax_invoice_id_idx ON public.case_in_doc_tax USING btree (invoice_id);
CREATE INDEX case_in_doc_tax_price31_idx ON public.case_in_doc_tax USING btree (price31);
CREATE INDEX case_in_doc_tax_price_idx ON public.case_in_doc_tax USING btree (price);
CREATE INDEX case_in_doc_tax_stamp_idx ON public.case_in_doc_tax USING btree (stamp);
CREATE INDEX case_in_doc_tax_type_idx ON public.case_in_doc_tax USING btree (type);


-- public.case_nap_status definition

-- Drop table

-- DROP TABLE public.case_nap_status;

CREATE TABLE public.case_nap_status (
	doc_id int8 NOT NULL,
	case_id int8 NOT NULL,
	sent timestamptz DEFAULT now() NOT NULL,
	received timestamptz NULL,
	responded timestamptz NULL,
	debt numeric(12, 3) DEFAULT 0 NOT NULL,
	currency_id int2 DEFAULT 1 NOT NULL,
	CONSTRAINT case_nap_status_pkey PRIMARY KEY (doc_id)
);
CREATE INDEX case_nap_status_case_id_idx ON public.case_nap_status USING btree (case_id);
CREATE INDEX case_nap_status_debt_idx ON public.case_nap_status USING btree (debt);
CREATE INDEX case_nap_status_doc_id_idx ON public.case_nap_status USING btree (doc_id);
CREATE INDEX case_nap_status_received_idx ON public.case_nap_status USING btree (received);
CREATE INDEX case_nap_status_responded_idx ON public.case_nap_status USING btree (responded);
CREATE INDEX case_nap_status_sent_idx ON public.case_nap_status USING btree (sent);


-- public.case_od_status definition

-- Drop table

-- DROP TABLE public.case_od_status;

CREATE TABLE public.case_od_status (
	st_id bigserial NOT NULL,
	doc_id int8 NOT NULL,
	stamp timestamptz DEFAULT now() NOT NULL,
	state int2 DEFAULT '-1'::integer NOT NULL,
	del_id int4 NULL,
	"comment" text NULL,
	estate int2 NULL,
	CONSTRAINT case_od_status_pkey PRIMARY KEY (st_id)
);
CREATE INDEX case_od_status_del_id_idx ON public.case_od_status USING btree (del_id);
CREATE INDEX case_od_status_doc_id_idx ON public.case_od_status USING btree (doc_id);
CREATE INDEX case_od_status_estate_idx ON public.case_od_status USING btree (estate);
CREATE INDEX case_od_status_stamp_idx ON public.case_od_status USING btree (stamp);
CREATE INDEX case_od_status_state_idx ON public.case_od_status USING btree (state);


-- public.case_out_doc definition

-- Drop table

-- DROP TABLE public.case_out_doc;

CREATE TABLE public.case_out_doc (
	doc_id serial4 NOT NULL,
	case_id int4 NULL,
	type_id int4 NULL,
	stype_id int4 NULL,
	doc_nr int4 DEFAULT 0 NOT NULL,
	created_id int4 NOT NULL,
	created timestamptz DEFAULT now() NOT NULL,
	"data" jsonb NULL,
	state int2 DEFAULT 0 NOT NULL,
	"comment" text NULL,
	debtor_id int4 NULL,
	creditor_id int4 NULL,
	price numeric(12, 3) DEFAULT 0::numeric NOT NULL,
	price31 numeric(12, 3) DEFAULT 0::numeric NOT NULL,
	"source" text NULL,
	modified_id int4 NULL,
	modified timestamptz DEFAULT now() NOT NULL,
	csdl jsonb DEFAULT '[]'::jsonb NOT NULL,
	itype_id int2 DEFAULT 0 NOT NULL,
	del_id int4 NULL,
	estate int2 NULL,
	numbered timestamptz NULL,
	invoice_id int8 NULL,
	account_id int8 NULL,
	ref_id int8 NULL,
	"shippedDate" timestamptz NULL,
	"resultDate" timestamptz NULL,
	shipping_in_days int2 NULL,
	shipped timestamptz NULL,
	CONSTRAINT case_out_doc_pkey PRIMARY KEY (doc_id)
);
CREATE INDEX case_out_doc_account_id_idx ON public.case_out_doc USING btree (account_id);
CREATE INDEX case_out_doc_case_id_idx ON public.case_out_doc USING btree (case_id);
CREATE INDEX case_out_doc_created_id_idx ON public.case_out_doc USING btree (created_id);
CREATE INDEX case_out_doc_created_idx ON public.case_out_doc USING btree (created);
CREATE INDEX case_out_doc_creditor_id_idx ON public.case_out_doc USING btree (creditor_id);
CREATE INDEX case_out_doc_csdl_idx ON public.case_out_doc USING btree (csdl);
CREATE INDEX case_out_doc_csdl_idx1 ON public.case_out_doc USING btree (csdl);
CREATE INDEX case_out_doc_debtor_id_idx ON public.case_out_doc USING btree (debtor_id);
CREATE INDEX case_out_doc_doc_nr_idx ON public.case_out_doc USING btree (doc_nr);
CREATE INDEX case_out_doc_estate_idx ON public.case_out_doc USING btree (estate);
CREATE INDEX case_out_doc_invoice_id_idx ON public.case_out_doc USING btree (invoice_id);
CREATE INDEX case_out_doc_modified_id_idx ON public.case_out_doc USING btree (modified_id);
CREATE INDEX case_out_doc_modified_idx ON public.case_out_doc USING btree (modified);
CREATE INDEX case_out_doc_numbered_idx ON public.case_out_doc USING btree (numbered);
CREATE INDEX case_out_doc_ref_id_idx ON public.case_out_doc USING btree (ref_id);
CREATE INDEX "case_out_doc_resultDate_idx" ON public.case_out_doc USING btree ("resultDate");
CREATE INDEX "case_out_doc_shippedDate_idx" ON public.case_out_doc USING btree ("shippedDate");
CREATE INDEX "case_out_doc_shippedDate_idx1" ON public.case_out_doc USING btree ("shippedDate");
CREATE INDEX case_out_doc_shipped_idx ON public.case_out_doc USING btree (shipped);
CREATE INDEX case_out_doc_shipping_in_days_idx ON public.case_out_doc USING btree (shipping_in_days);
CREATE INDEX case_out_doc_shipping_in_days_idx1 ON public.case_out_doc USING btree (shipping_in_days);
CREATE INDEX case_out_doc_state_idx ON public.case_out_doc USING btree (state);
CREATE INDEX case_out_doc_stype_id_idx ON public.case_out_doc USING btree (stype_id);
CREATE INDEX case_out_doc_type_id_idx ON public.case_out_doc USING btree (type_id);


-- public.case_out_doc_attach definition

-- Drop table

-- DROP TABLE public.case_out_doc_attach;

CREATE TABLE public.case_out_doc_attach (
	att_id bigserial NOT NULL,
	doc_id int8 NOT NULL,
	"source" varchar(120) NULL,
	user_id int4 NOT NULL,
	stamp timestamptz DEFAULT now() NOT NULL,
	"comment" text NULL,
	CONSTRAINT case_out_doc_attach_pkey PRIMARY KEY (att_id)
);
CREATE INDEX case_out_doc_attach_doc_id_idx ON public.case_out_doc_attach USING btree (doc_id);
CREATE INDEX case_out_doc_attach_stamp_idx ON public.case_out_doc_attach USING btree (stamp);
CREATE INDEX case_out_doc_attach_user_id_idx ON public.case_out_doc_attach USING btree (user_id);


-- public.case_pdi_status definition

-- Drop table

-- DROP TABLE public.case_pdi_status;

CREATE TABLE public.case_pdi_status (
	doc_id int8 NOT NULL,
	case_id int8 NOT NULL,
	sent timestamptz DEFAULT now() NOT NULL,
	received timestamptz NULL,
	CONSTRAINT case_pdi_status_pkey PRIMARY KEY (doc_id)
);
CREATE INDEX case_pdi_status_case_id_idx ON public.case_pdi_status USING btree (case_id);
CREATE INDEX case_pdi_status_doc_id_idx ON public.case_pdi_status USING btree (doc_id);
CREATE INDEX case_pdi_status_received_idx ON public.case_pdi_status USING btree (received);
CREATE INDEX case_pdi_status_sent_idx ON public.case_pdi_status USING btree (sent);


-- public.case_person_pdi definition

-- Drop table

-- DROP TABLE public.case_person_pdi;

CREATE TABLE public.case_person_pdi (
	case_id int4 NOT NULL,
	person_id int4 NOT NULL,
	pdi bool DEFAULT false NOT NULL,
	doc_id int8 NULL,
	sent timestamptz NULL,
	received timestamptz NULL,
	status int2 DEFAULT 0 NOT NULL,
	CONSTRAINT case_person_pdi_pkey PRIMARY KEY (case_id, person_id)
);
CREATE INDEX case_person_pdi_case_id_idx ON public.case_person_pdi USING btree (case_id);
CREATE INDEX case_person_pdi_doc_id_idx ON public.case_person_pdi USING btree (doc_id);
CREATE INDEX case_person_pdi_person_id_idx ON public.case_person_pdi USING btree (person_id);
CREATE INDEX case_person_pdi_received_idx ON public.case_person_pdi USING btree (received);
CREATE INDEX case_person_pdi_sent_idx ON public.case_person_pdi USING btree (sent);
CREATE INDEX case_person_pdi_status_idx ON public.case_person_pdi USING btree (status);


-- public.cases_ces_action definition

-- Drop table

-- DROP TABLE public.cases_ces_action;

CREATE TABLE public.cases_ces_action (
	att_id int4 NOT NULL,
	case_id int4 NOT NULL,
	act_stamp timestamptz NULL,
	exec_id int4 NULL,
	old_cred_id int4 NULL,
	new_cred_id int4 NULL,
	approved bool DEFAULT false NOT NULL,
	user_id int4 NULL,
	ctrct_stamp timestamptz NULL,
	CONSTRAINT cases_ces_action_pkey PRIMARY KEY (att_id)
);
CREATE INDEX cases_ces_action_act_stamp_idx ON public.cases_ces_action USING btree (act_stamp);
CREATE INDEX cases_ces_action_approved_idx ON public.cases_ces_action USING btree (approved);
CREATE INDEX cases_ces_action_att_id_idx ON public.cases_ces_action USING btree (att_id);
CREATE INDEX cases_ces_action_case_id_idx ON public.cases_ces_action USING btree (case_id);
CREATE INDEX cases_ces_action_ctrct_stamp_idx ON public.cases_ces_action USING btree (ctrct_stamp);
CREATE INDEX cases_ces_action_exec_id_idx ON public.cases_ces_action USING btree (exec_id);
CREATE INDEX cases_ces_action_user_id_idx ON public.cases_ces_action USING btree (user_id);


-- public.m18_payment definition

-- Drop table

-- DROP TABLE public.m18_payment;

CREATE TABLE public.m18_payment (
	pay_id bigserial NOT NULL,
	arr_id int8 NOT NULL,
	att_id int8 NOT NULL,
	amount numeric(12, 3) DEFAULT 0 NOT NULL,
	currency_id int2 DEFAULT 1 NOT NULL,
	user_id int4 NOT NULL,
	stamp timestamptz DEFAULT now() NOT NULL,
	invoice_id int4 NULL,
	"comment" text NULL,
	CONSTRAINT m18_payment_pkey PRIMARY KEY (pay_id)
);
CREATE INDEX m18_payment_amount_idx ON public.m18_payment USING btree (amount);
CREATE INDEX m18_payment_arr_id_idx ON public.m18_payment USING btree (arr_id);
CREATE INDEX m18_payment_att_id_idx ON public.m18_payment USING btree (att_id);
CREATE INDEX m18_payment_invoice_id_idx ON public.m18_payment USING btree (invoice_id);
CREATE INDEX m18_payment_stamp_idx ON public.m18_payment USING btree (stamp);
CREATE INDEX m18_payment_user_id_idx ON public.m18_payment USING btree (user_id);


-- public.case_attach foreign keys

ALTER TABLE public.case_attach ADD CONSTRAINT case_attach_case_id_fkey FOREIGN KEY (case_id) REFERENCES public.case_case(case_id) ON DELETE CASCADE;
ALTER TABLE public.case_attach ADD CONSTRAINT case_attach_creditor_id_fkey FOREIGN KEY (creditor_id) REFERENCES public.case_person(person_id) ON DELETE RESTRICT;
ALTER TABLE public.case_attach ADD CONSTRAINT case_attach_debtor_id_fkey FOREIGN KEY (debtor_id) REFERENCES public.case_person(person_id) ON DELETE RESTRICT;
ALTER TABLE public.case_attach ADD CONSTRAINT case_attach_out_doc_id_fkey FOREIGN KEY (out_doc_id) REFERENCES public.case_out_doc(doc_id) ON DELETE CASCADE;
ALTER TABLE public.case_attach ADD CONSTRAINT case_attach_parent_id FOREIGN KEY (parent_id) REFERENCES public.case_attach(att_id) ON DELETE CASCADE;
ALTER TABLE public.case_attach ADD CONSTRAINT case_attach_person_id_fkey FOREIGN KEY (person_id) REFERENCES public.case_person(person_id) ON DELETE RESTRICT;


-- public.case_in_doc_tax foreign keys

ALTER TABLE public.case_in_doc_tax ADD CONSTRAINT case_in_doc_tax_account_id_fkey FOREIGN KEY (account_id) REFERENCES public.case_account(acc_id);
ALTER TABLE public.case_in_doc_tax ADD CONSTRAINT case_in_doc_tax_att_id_fkey FOREIGN KEY (att_id) REFERENCES public.case_attach(att_id) ON DELETE CASCADE;


-- public.case_nap_status foreign keys

ALTER TABLE public.case_nap_status ADD CONSTRAINT case_nap_status_case_id_fkey FOREIGN KEY (case_id) REFERENCES public.case_case(case_id) ON DELETE CASCADE;
ALTER TABLE public.case_nap_status ADD CONSTRAINT case_nap_status_doc_id_fkey FOREIGN KEY (doc_id) REFERENCES public.case_out_doc(doc_id) ON DELETE CASCADE;


-- public.case_od_status foreign keys

ALTER TABLE public.case_od_status ADD CONSTRAINT case_od_status_doc_id_fkey FOREIGN KEY (doc_id) REFERENCES public.case_out_doc(doc_id) ON DELETE CASCADE;


-- public.case_out_doc foreign keys

ALTER TABLE public.case_out_doc ADD CONSTRAINT case_out_doc_account_id_fkey FOREIGN KEY (account_id) REFERENCES public.case_account(acc_id);
ALTER TABLE public.case_out_doc ADD CONSTRAINT case_out_doc_case_id_fkey FOREIGN KEY (case_id) REFERENCES public.case_case(case_id) ON DELETE CASCADE;
ALTER TABLE public.case_out_doc ADD CONSTRAINT case_out_doc_creditor_id_fkey FOREIGN KEY (creditor_id) REFERENCES public.case_person(person_id) ON DELETE RESTRICT;
ALTER TABLE public.case_out_doc ADD CONSTRAINT case_out_doc_debtor_id_fkey FOREIGN KEY (debtor_id) REFERENCES public.case_person(person_id) ON DELETE RESTRICT;
ALTER TABLE public.case_out_doc ADD CONSTRAINT case_out_doc_del_id_fkey FOREIGN KEY (del_id) REFERENCES public.case_deliverer(del_id) ON DELETE RESTRICT;
ALTER TABLE public.case_out_doc ADD CONSTRAINT case_out_doc_ref_id_fkey FOREIGN KEY (ref_id) REFERENCES public.case_attach(att_id) ON DELETE CASCADE;
ALTER TABLE public.case_out_doc ADD CONSTRAINT case_out_doc_stype_id_fkey FOREIGN KEY (stype_id) REFERENCES public.case_doc_sub_type(stype_id) ON DELETE RESTRICT;
ALTER TABLE public.case_out_doc ADD CONSTRAINT case_out_doc_type_id_fkey FOREIGN KEY (type_id) REFERENCES public.case_doc_type(type_id) ON DELETE RESTRICT;


-- public.case_out_doc_attach foreign keys

ALTER TABLE public.case_out_doc_attach ADD CONSTRAINT case_out_doc_attach_doc_id_fkey FOREIGN KEY (doc_id) REFERENCES public.case_out_doc(doc_id) ON DELETE CASCADE;
ALTER TABLE public.case_out_doc_attach ADD CONSTRAINT case_out_doc_attach_user_id FOREIGN KEY (user_id) REFERENCES public."user"(user_id) ON DELETE RESTRICT;


-- public.case_pdi_status foreign keys

ALTER TABLE public.case_pdi_status ADD CONSTRAINT case_pdi_status_case_id_fkey FOREIGN KEY (case_id) REFERENCES public.case_case(case_id) ON DELETE CASCADE;
ALTER TABLE public.case_pdi_status ADD CONSTRAINT case_pdi_status_doc_id_fkey FOREIGN KEY (doc_id) REFERENCES public.case_out_doc(doc_id) ON DELETE CASCADE;


-- public.case_person_pdi foreign keys

ALTER TABLE public.case_person_pdi ADD CONSTRAINT case_person_pdi_case_id_fkey FOREIGN KEY (case_id) REFERENCES public.case_case(case_id) ON DELETE CASCADE;
ALTER TABLE public.case_person_pdi ADD CONSTRAINT case_person_pdi_doc_id_fkey FOREIGN KEY (doc_id) REFERENCES public.case_out_doc(doc_id) ON DELETE SET NULL;
ALTER TABLE public.case_person_pdi ADD CONSTRAINT case_person_pdi_person_id_fkey FOREIGN KEY (person_id) REFERENCES public.case_person(person_id) ON DELETE CASCADE;


-- public.cases_ces_action foreign keys

ALTER TABLE public.cases_ces_action ADD CONSTRAINT cases_ces_action_att_id_fkey FOREIGN KEY (att_id) REFERENCES public.case_attach(att_id);
ALTER TABLE public.cases_ces_action ADD CONSTRAINT cases_ces_action_case_id_fkey FOREIGN KEY (case_id) REFERENCES public.case_case(case_id) ON DELETE CASCADE;
ALTER TABLE public.cases_ces_action ADD CONSTRAINT cases_ces_action_exec_id_fkey FOREIGN KEY (exec_id) REFERENCES public.case_execution_list(exec_id) ON DELETE CASCADE;
ALTER TABLE public.cases_ces_action ADD CONSTRAINT cases_ces_action_new_cred_id_fkey FOREIGN KEY (new_cred_id) REFERENCES public.case_person(person_id) ON DELETE RESTRICT;
ALTER TABLE public.cases_ces_action ADD CONSTRAINT cases_ces_action_old_cred_id_fkey FOREIGN KEY (old_cred_id) REFERENCES public.case_person(person_id) ON DELETE RESTRICT;


-- public.m18_payment foreign keys

ALTER TABLE public.m18_payment ADD CONSTRAINT m18_payment_arr_id_fkey FOREIGN KEY (arr_id) REFERENCES public.case_arrived_money(arr_id) ON DELETE RESTRICT;
ALTER TABLE public.m18_payment ADD CONSTRAINT m18_payment_att_id_fkey FOREIGN KEY (att_id) REFERENCES public.case_attach(att_id) ON DELETE RESTRICT;
ALTER TABLE public.m18_payment ADD CONSTRAINT m18_payment_invoice_id FOREIGN KEY (invoice_id) REFERENCES public.invoice(invoice_id) ON DELETE SET NULL;
ALTER TABLE public.m18_payment ADD CONSTRAINT m18_payment_user_id FOREIGN KEY (user_id) REFERENCES public."user"(user_id) ON DELETE RESTRICT;