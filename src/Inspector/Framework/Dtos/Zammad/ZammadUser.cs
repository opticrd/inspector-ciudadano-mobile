using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inspector.Framework.Dtos.Zammad
{
    public partial class ZammadUser
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("organization_id")]
        public string OrganizationId { get; set; }

        [JsonProperty("organization")]
        public string Organization { get; set; }

        [JsonProperty("login")]
        public string Login { get; set; }

        [JsonProperty("firstname")]
        public string Firstname { get; set; }

        [JsonProperty("lastname")]
        public string Lastname { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("image")]
        public object Image { get; set; }

        [JsonProperty("image_source")]
        public object ImageSource { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("web")]
        public string Web { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("fax")]
        public string Fax { get; set; }

        [JsonProperty("mobile")]
        public string Mobile { get; set; }

        [JsonProperty("department")]
        public string Department { get; set; }

        [JsonProperty("street")]
        public string Street { get; set; }

        [JsonProperty("zip")]
        public string Zip { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("address")]
        public object Address { get; set; }

        [JsonProperty("vip")]
        public bool Vip { get; set; }

        [JsonProperty("verified")]
        public bool Verified { get; set; }

        [JsonProperty("active")]
        public bool Active { get; set; }

        [JsonProperty("note")]
        public string Note { get; set; }

        [JsonProperty("last_login")]
        public DateTimeOffset LastLogin { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("login_failed")]
        public long LoginFailed { get; set; }

        [JsonProperty("out_of_office")]
        public bool OutOfOffice { get; set; }

        [JsonProperty("out_of_office_start_at")]
        public object OutOfOfficeStartAt { get; set; }

        [JsonProperty("out_of_office_end_at")]
        public object OutOfOfficeEndAt { get; set; }

        [JsonProperty("out_of_office_replacement_id")]
        public object OutOfOfficeReplacementId { get; set; }

        [JsonProperty("updated_by_id")]
        public long UpdatedById { get; set; }

        [JsonProperty("created_by_id")]
        public long CreatedById { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("cedula")]
        public string Cedula { get; set; }

        [JsonProperty("role_ids")]
        public List<int> RoleIds { get; set; }

        [JsonProperty("authorization_ids")]
        public List<int> AuthorizationIds { get; set; }

        [JsonProperty("karma_user_ids")]
        public List<object> KarmaUserIds { get; set; }

    }
}
