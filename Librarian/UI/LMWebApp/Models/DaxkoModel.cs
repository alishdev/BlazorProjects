
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LMWebApp.Models;
public class DaxkoOffering
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("start_date")]
    public DateTime StartDate { get; set; }

    [JsonPropertyName("end_date")]
    public DateTime EndDate { get; set; }

    [JsonPropertyName("program")]
    public DaxkoProgram DaxkoProgram { get; set; } = new();

    [JsonPropertyName("locations")]
    public List<DaxkoLocation> Locations { get; set; } = new();

    [JsonPropertyName("categories")]
    public List<DaxkoCategory> Categories { get; set; } = new();

    [JsonPropertyName("groups")]
    public List<DaxkoGroup> Groups { get; set; } = new();

    [JsonPropertyName("restrictions")]
    public DaxkoRestrictions DaxkoRestrictions { get; set; } = new();

    [JsonPropertyName("times")]
    public List<DaxkoTimeSlot> Times { get; set; } = new();

    [JsonPropertyName("days_offered")]
    public List<DaxkoDayOffered> DaysOffered { get; set; } = new();

    [JsonPropertyName("highlights")]
    public List<string> Highlights { get; set; } = new();

    [JsonPropertyName("registration")]
    public DaxkoRegistration DaxkoRegistration { get; set; } = new();

    [JsonPropertyName("score")]
    public int Score { get; set; }
}

public class DaxkoProgram
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}

public class DaxkoLocation
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}

public class DaxkoCategory
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class DaxkoGroup
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("rate")]
    public DaxkoRate DaxkoRate { get; set; } = new();
}

public class DaxkoRate
{
    [JsonPropertyName("min_amount")]
    public decimal MinAmount { get; set; }

    [JsonPropertyName("max_amount")]
    public decimal MaxAmount { get; set; }

    [JsonPropertyName("frequency")]
    public string Frequency { get; set; } = string.Empty;

    [JsonPropertyName("unit")]
    public string Unit { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}

public class DaxkoRestrictions
{
    [JsonPropertyName("genders")]
    public List<DaxkoGender> Genders { get; set; } = new();
}

public class DaxkoGender
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class DaxkoTimeSlot
{
    [JsonPropertyName("start")]
    public string Start { get; set; } = string.Empty;

    [JsonPropertyName("end")]
    public string End { get; set; } = string.Empty;
}

public class DaxkoDayOffered
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class DaxkoRegistration
{
    [JsonPropertyName("start")]
    public DateTime Start { get; set; }

    [JsonPropertyName("end")]
    public DateTime End { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;
}

// Response wrapper class for the API response
public class DaxkoOfferingsResponse
{
    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("limit")]
    public int Limit { get; set; }

    [JsonPropertyName("offerings")]
    public List<DaxkoOffering> Offerings { get; set; } = new();
}

// Daxko Member Models
public class DaxkoMember
{
    [JsonPropertyName("platform")]
    public string Platform { get; set; } = string.Empty;

    [JsonPropertyName("client_id")]
    public string ClientId { get; set; } = string.Empty;

    [JsonPropertyName("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("middle_name")]
    public string MiddleName { get; set; } = string.Empty;

    [JsonPropertyName("last_name")]
    public string LastName { get; set; } = string.Empty;

    [JsonPropertyName("date_of_birth")]
    public DateTime DateOfBirth { get; set; }

    [JsonPropertyName("gender")]
    public string Gender { get; set; } = string.Empty;

    [JsonPropertyName("member_id")]
    public string MemberId { get; set; } = string.Empty;

    [JsonPropertyName("barcode")]
    public string Barcode { get; set; } = string.Empty;

    [JsonPropertyName("membership_type")]
    public string MembershipType { get; set; } = string.Empty;

    [JsonPropertyName("is_primary_member")]
    public bool IsPrimaryMember { get; set; }

    [JsonPropertyName("active")]
    public bool Active { get; set; }

    [JsonPropertyName("join_date")]
    public DateTime JoinDate { get; set; }

    [JsonPropertyName("most_recent_join_date")]
    public DateTime MostRecentJoinDate { get; set; }

    [JsonPropertyName("phones")]
    public List<DaxkoPhone> Phones { get; set; } = new();

    [JsonPropertyName("emails")]
    public List<DaxkoEmail> Emails { get; set; } = new();

    [JsonPropertyName("addresses")]
    public List<DaxkoAddress> Addresses { get; set; } = new();

    [JsonPropertyName("last_updated_utc")]
    public DateTime LastUpdatedUtc { get; set; }

    [JsonPropertyName("additional_details")]
    public DaxkoAdditionalDetails AdditionalDetails { get; set; } = new();
}

public class DaxkoPhone
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("phone")]
    public string Phone { get; set; } = string.Empty;
}

public class DaxkoEmail
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
}

public class DaxkoAddress
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("address")]
    public string Address { get; set; } = string.Empty;

    [JsonPropertyName("address2")]
    public string Address2 { get; set; } = string.Empty;

    [JsonPropertyName("city")]
    public string City { get; set; } = string.Empty;

    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;

    [JsonPropertyName("zip_code")]
    public string ZipCode { get; set; } = string.Empty;

    [JsonPropertyName("country")]
    public string Country { get; set; } = string.Empty;
}

public class DaxkoAdditionalDetails
{
    [JsonPropertyName("unit_id")]
    public string UnitId { get; set; } = string.Empty;
}

// Response wrapper class for the member search API response
public class DaxkoMembersResponse
{
    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("has_more_records")]
    public bool HasMoreRecords { get; set; }

    [JsonPropertyName("members")]
    public List<DaxkoMember> Members { get; set; } = new();
}
