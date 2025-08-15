using AdoExport.Api.Controllers.Models;
using AdoExport.Api.Repositories;

namespace AdoExport.Api.Controllers.Sorting;

public static class SortingExtensions
{
    /// <summary>
    /// Orders the given <paramref name="userEntries" /> based on the total of affected user and pat rules descending by severity.<br />
    /// An item with a higher total count of critical results would precede an item with a lower total count of critical results. 
    /// </summary>
    /// <param name="userEntries">The user entries to order.</param>
    /// <returns>An ordered enumerable of the given <paramref name="userEntries" />.</returns>
    public static IOrderedEnumerable<UserEntryDto> OrderByTotals(this IEnumerable<UserEntryDto> userEntries)
    {
        return userEntries
            .OrderByDescending(user => user.AffectedUserRulesCounts.TotalCriticals)
            .ThenByDescending(user => user.AffectedUserRulesCounts.TotalWarnings)
            .ThenByDescending(user => user.AffectedUserRulesCounts.TotalOks)
            .ThenBy(user => user.UserName);
    }

    /// <summary>
    /// Orders the given <paramref name="userEntries" /> based on the total of affected user and pat rules descending by severity.<br />
    /// An item with a higher total count of critical results would precede an item with a lower total count of critical results. 
    /// </summary>
    /// <param name="userEntries">The user entries to order.</param>
    /// <returns>An ordered enumerable of the given <paramref name="userEntries" />.</returns>
    public static IOrderedEnumerable<UserEntryDto> OrderByName(this IEnumerable<UserEntryDto> userEntries)
    {
        return userEntries
            .OrderBy(user => user.UserName);
    }

    /// <summary>
    /// Orders the given <paramref name="usersById" /> based on the total of affected pat rules descending by severity.<br />
    /// An item with a higher total count of critical results would precede an item with a lower total count of critical results. 
    /// </summary>
    /// <param name="usersById">The user by id entries to order.</param>
    /// <returns>An ordered enumerable of the given <paramref name="usersById" />.</returns>
    public static IOrderedEnumerable<GetUserById> OrderByName(this IEnumerable<GetUserById> usersById)
    {
        return usersById
            .OrderBy(userById => userById.User.UserName);
    }

    /// <summary>
    /// Orders the given <paramref name="userRuleDetails" /> based on the total of affected rules descending by severity.<br />
    /// An item with a higher total count of critical results would precede an item with a lower total count of critical results. 
    /// </summary>
    /// <param name="userRuleDetails">The rule results to order.</param>
    /// <returns>An ordered enumerable of the given <paramref name="userRuleDetails" />.</returns>
    public static IOrderedEnumerable<UserRuleDetailsDto> OrderByTotals(this IEnumerable<UserRuleDetailsDto> userRuleDetails)
    {
        return userRuleDetails
            .OrderByDescending(user => user.AffectedUserRulesCounts.TotalCriticals)
            .ThenByDescending(user => user.AffectedUserRulesCounts.TotalWarnings)
            .ThenByDescending(user => user.AffectedUserRulesCounts.TotalOks)
            .ThenBy(user => user.UserName);
    }

    /// <summary>
    /// Orders the given <paramref name="patByIds" /> based on the total of affected rules descending by severity.<br />
    /// An item with a higher total count of critical results would precede an item with a lower total count of critical results. 
    /// </summary>
    /// <param name="patByIds">The pat results to order.</param>
    /// <returns>An ordered enumerable of the given <paramref name="patByIds" />.</returns>
    public static IOrderedEnumerable<GetPatById> OrderByTotals(this IEnumerable<GetPatById> patByIds)
    {
        return patByIds
            .OrderByDescending(pat => pat.Pat.AffectedPatRulesCounts.TotalCriticals)
            .ThenByDescending(pat => pat.Pat.AffectedPatRulesCounts.TotalWarnings)
            .ThenByDescending(pat => pat.Pat.AffectedPatRulesCounts.TotalOks);
    }

    /// <summary>
    /// Orders the given <paramref name="patResults" /> based on the total of affected rules descending by severity.<br />
    /// An item with a higher total count of critical results would precede an item with a lower total count of critical results. 
    /// </summary>
    /// <param name="patResults">The pat results to order.</param>
    /// <returns>An ordered enumerable of the given <paramref name="patResults" />.</returns>
    public static IOrderedEnumerable<PatDetailsDto> OrderByTotals(this IEnumerable<PatDetailsDto> patResults)
    {
        return patResults
            .OrderByDescending(rule => rule.AffectedPatRulesCounts.TotalCriticals)
            .ThenByDescending(rule => rule.AffectedPatRulesCounts.TotalWarnings)
            .ThenByDescending(rule => rule.AffectedPatRulesCounts.TotalOks);
    }

    /// <summary>
    /// Orders the given <paramref name="rules" /> based on their severity.<br />
    /// An item with a higher severity would precede an item with a lower severity. 
    /// </summary>
    /// <param name="rules">The rules to order.</param>
    /// <returns>An ordered enumerable of the given <paramref name="rules" />.</returns>
    public static IOrderedEnumerable<RuleDto> OrderBySeverity(this IEnumerable<RuleDto> rules)
    {
        return rules
            .OrderBy(rule => rule.Severity)
            .ThenByDescending(rule => rule.TotalCounts);
    }

    /// <summary>
    /// Orders the given <paramref name="rules" /> based on their name (a-z).
    /// </summary>
    /// <param name="rules">The rules to order.</param>
    /// <returns>An ordered enumerable of the given <paramref name="rules" />.</returns>
    public static IOrderedEnumerable<RuleDto> OrderByName(this IEnumerable<RuleDto> rules)
    {
        return rules
            .OrderBy(rule => rule.Name);
    }
}