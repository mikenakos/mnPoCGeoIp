----------------------------------------------------------------------------------------
-- Header table
create table geoip_lookup_batches (
	id  bigint identity(1,1) not null,
	created datetimeoffset not null default GETUTCDATE(),
	process_started datetimeoffset null,
	process_ended datetimeoffset null,
	batch_count smallint not null default 0,
);

alter table geoip_lookup_batches
	add constraint pk_geoip_lookup_batches primary key (id);

create index ndx_proc_end on geoip_lookup_batches (process_ended);

----------------------------------------------------------------------------------------
-- Details lookup table
create table geoip_lookup_ips (
	glb_id bigint not null,
	exec_order smallint not null,
	ip_address nvarchar(45) not null
);

alter table geoip_lookup_ips
	add constraint pk_geoip_lookup_ips primary key (glb_id, exec_order);

alter table geoip_lookup_ips
	add constraint fkd_geoip_lookup_ips_glb_id
	foreign key (glb_id) references geoip_lookup_batches(id)
	on delete cascade;
----------------------------------------------------------------------------------------
-- Lookup results table
create table geoip_lookup_ips_results (
	glb_id bigint not null,
	exec_order smallint not null,
	updated datetimeoffset not null default GETUTCDATE(),
	last_error nvarchar(128) null,
	country_iso3_code char(3) null,
	country_name nvarchar(96) null,
	timezone_id nvarchar(64) not null,
	lat decimal(8, 6) not null,
	lon decimal(9, 6) not null
);

alter table geoip_lookup_ips_results
	add constraint pk_geoip_lookup_ips_results primary key (glb_id, exec_order);

alter table geoip_lookup_ips_results
	add constraint fkd_geoip_lookup_ips_results_glb_id
	foreign key (glb_id, exec_order) references geoip_lookup_ips(glb_id, exec_order)
	on delete cascade;
----------------------------------------------------------------------------------------
