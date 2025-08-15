using AdoExport.Api.Controllers.Models;
using AdoExport.Api.Controllers.Sorting;
using AdoExport.Api.Repositories;

namespace AdoExport.Api.Tests.Controllers.Sorting;

[TestFixture]
public static class SortingExtensionsTests
{
    [TestFixture]
    public class PatDetailsDtoSortingTests
    {
        [Test]
        public void OrderByTotals_SortsByHighestCriticalsFirst()
        {
            var fixture = CreateFixture();

            var patDetailsA = CreatePatDetailsEntry(fixture, "Pat A", 1, 1, 1);
            var patDetailsB = CreatePatDetailsEntry(fixture, "Pat B", 0, 1, 1);
            var patDetailsC = CreatePatDetailsEntry(fixture, "Pat C", 12, 1, 1);

            new List<PatDetailsDto>
                {
                    patDetailsA,
                    patDetailsB,
                    patDetailsC,
                }
                .OrderByTotals()
                .ToArray()
                .Should()
                .Equal([
                    patDetailsC,
                    patDetailsA,
                    patDetailsB,
                ]);
        }

        [Test]
        public void OrderByTotals_SortsByHighestWarningsSecond()
        {
            var fixture = CreateFixture();

            var patDetailsA = CreatePatDetailsEntry(fixture, "Pat A", 1, 12, 1);
            var patDetailsB = CreatePatDetailsEntry(fixture, "Pat B", 0, 12, 1);
            var patDetailsC = CreatePatDetailsEntry(fixture, "Pat C", 1, 11, 1);

            new List<PatDetailsDto>
                {
                    patDetailsA,
                    patDetailsB,
                    patDetailsC,
                }
                .OrderByTotals()
                .ToArray()
                .Should()
                .Equal([
                    patDetailsA,
                    patDetailsC,
                    patDetailsB,
                ]);
        }

        [Test]
        public void OrderByTotals_SortsByHighestOksThird()
        {
            var fixture = CreateFixture();

            var patDetailsA = CreatePatDetailsEntry(fixture, "Pat A", 1, 12, 1);
            var patDetailsB = CreatePatDetailsEntry(fixture, "Pat B", 1, 12, 12);
            var patDetailsC = CreatePatDetailsEntry(fixture, "Pat C", 1, 12, 13);

            new List<PatDetailsDto>
                {
                    patDetailsA,
                    patDetailsB,
                    patDetailsC,
                }
                .OrderByTotals()
                .ToArray()
                .Should()
                .Equal([
                    patDetailsC,
                    patDetailsB,
                    patDetailsA,
                ]);
        }
    }

    [TestFixture]
    public class RulesDtoSortingTests
    {
        private static IEnumerable<TestCaseData> EntityCategories()
        {
            return Enum.GetValues<Categories>().Select(value => new TestCaseData(value));
        }

        [TestCaseSource(nameof(EntityCategories))]
        public void OrderBySeverity_SortsByHighestSeverityFirst(Categories category)
        {
            var fixture = CreateFixture();

            var ruleA = CreateRuleDtoEntry(fixture, "Rule A", category, Severities.Ok);
            var ruleB = CreateRuleDtoEntry(fixture, "Rule B", category, Severities.Critical);
            var ruleC = CreateRuleDtoEntry(fixture, "Rule C", category, Severities.Warning);

            new List<RuleDto>
                {
                    ruleA,
                    ruleB,
                    ruleC,
                }
                .OrderBySeverity()
                .ToArray()
                .Should()
                .Equal([
                    ruleB,
                    ruleC,
                    ruleA,
                ]);
        }

        [TestCaseSource(nameof(EntityCategories))]
        public void OrderBySeverity_SortsByTotalCountSecond(Categories category)
        {
            var fixture = CreateFixture();

            var ruleA = CreateRuleDtoEntry(fixture, "Rule A", category, Severities.Critical, 1);
            var ruleB = CreateRuleDtoEntry(fixture, "Rule B", category, Severities.Critical, 12);
            var ruleC = CreateRuleDtoEntry(fixture, "Rule C", category, Severities.Ok, 3000);
            var ruleD = CreateRuleDtoEntry(fixture, "Rule D", category, Severities.Ok, 10000);

            new List<RuleDto>
                {
                    ruleA,
                    ruleB,
                    ruleC,
                    ruleD,
                }
                .OrderBySeverity()
                .ToArray()
                .Should()
                .Equal([
                    ruleB,
                    ruleA,
                    ruleD,
                    ruleC,
                ]);
        }

        [TestCaseSource(nameof(EntityCategories))]
        public void OrderByName_SortsByNameAlphabetically(Categories category)
        {
            var fixture = CreateFixture();

            var ruleA = CreateRuleDtoEntry(fixture, "Rule A", category);
            var ruleB = CreateRuleDtoEntry(fixture, "Rule B", category);
            var ruleC = CreateRuleDtoEntry(fixture, "Rule C", category);

            new List<RuleDto>
                {
                    ruleB,
                    ruleA,
                    ruleC,
                }
                .OrderByName()
                .ToArray()
                .Should()
                .Equal([
                    ruleA,
                    ruleB,
                    ruleC,
                ]);
        }
    }

    [TestFixture]
    public class UserEntryDtoSortingTests
    {
        [Test]
        public void OrderByTotals_SortsByHighestCriticalFirst()
        {
            var fixture = CreateFixture();

            var userEntryA = CreateUserEntry(fixture, "User A", 1, 0, 0);
            var userEntryB = CreateUserEntry(fixture, "User B", 3, 0, 0);
            var userEntryC = CreateUserEntry(fixture, "User C", 2, 0, 0);

            new List<UserEntryDto>
                {
                    userEntryA,
                    userEntryB,
                    userEntryC,
                }
                .OrderByTotals()
                .ToArray()
                .Should()
                .Equal([
                    userEntryB,
                    userEntryC,
                    userEntryA,
                ]);
        }

        [Test]
        public void OrderByTotals_SortsByHighestWarningsSecond()
        {
            var fixture = CreateFixture();

            var userEntryA = CreateUserEntry(fixture, "User A", 10, 1, 0);
            var userEntryB = CreateUserEntry(fixture, "User B", 10, 3, 0);
            var userEntryC = CreateUserEntry(fixture, "User C", 10, 2, 0);

            new List<UserEntryDto>
                {
                    userEntryA,
                    userEntryB,
                    userEntryC,
                }
                .OrderByTotals()
                .ToArray()
                .Should()
                .Equal([
                    userEntryB,
                    userEntryC,
                    userEntryA,
                ]);
        }

        [Test]
        public void OrderByTotals_SortsByHighestOksThird()
        {
            var fixture = CreateFixture();

            var userEntryA = CreateUserEntry(fixture, "User A", 10, 10, 1);
            var userEntryB = CreateUserEntry(fixture, "User B", 10, 10, 3);
            var userEntryC = CreateUserEntry(fixture, "User C", 10, 10, 2);

            new List<UserEntryDto>
                {
                    userEntryA,
                    userEntryB,
                    userEntryC,
                }
                .OrderByTotals()
                .ToArray()
                .Should()
                .Equal([
                    userEntryB,
                    userEntryC,
                    userEntryA,
                ]);
        }

        [Test]
        public void OrderByTotals_SortsByUserNameFourths()
        {
            var fixture = CreateFixture();

            var userEntryA = CreateUserEntry(fixture, "User A", 10, 10, 10);
            var userEntryB = CreateUserEntry(fixture, "User B", 10, 10, 10);
            var userEntryC = CreateUserEntry(fixture, "User C", 10, 10, 10);

            new List<UserEntryDto>
                {
                    userEntryA,
                    userEntryB,
                    userEntryC,
                }
                .OrderByTotals()
                .ToArray()
                .Should()
                .Equal([
                    userEntryA,
                    userEntryB,
                    userEntryC,
                ]);
        }

        [Test]
        public void OrderByName_SortsByUserName()
        {
            var fixture = CreateFixture();

            var userEntryA = CreateUserEntry(fixture, "User A", 10, 10, 10);
            var userEntryB = CreateUserEntry(fixture, "User B", 10, 10, 10);
            var userEntryC = CreateUserEntry(fixture, "User C", 10, 10, 10);

            new List<UserEntryDto>
                {
                    userEntryB,
                    userEntryA,
                    userEntryC,
                }
                .OrderByName()
                .ToArray()
                .Should()
                .Equal([
                    userEntryA,
                    userEntryB,
                    userEntryC,
                ]);
        }
    }

    [TestFixture]
    public class UserRuleDetailsDtoSortingTests
    {
        [Test]
        public void OrderByTotals_SortsByHighestCriticalFirst()
        {
            var fixture = CreateFixture();

            var userRuleDetailsA = CreateUserRuleDetailsEntry(fixture, "User A", 10, 5, 1);
            var userRuleDetailsB = CreateUserRuleDetailsEntry(fixture, "User B", 11, 7, 1);
            var userRuleDetailsC = CreateUserRuleDetailsEntry(fixture, "User C", 10, 4, 1);

            new List<UserRuleDetailsDto>
                {
                    userRuleDetailsA,
                    userRuleDetailsB,
                    userRuleDetailsC,
                }
                .OrderByTotals()
                .ToArray()
                .Should()
                .Equal([
                    userRuleDetailsB,
                    userRuleDetailsA,
                    userRuleDetailsC,
                ]);
        }

        [Test]
        public void OrderByTotals_SortsByHighestWarningsSecond()
        {
            var fixture = CreateFixture();

            var userRuleDetailsA = CreateUserRuleDetailsEntry(fixture, "User A", 10, 5, 1);
            var userRuleDetailsB = CreateUserRuleDetailsEntry(fixture, "User B", 10, 7, 1);
            var userRuleDetailsC = CreateUserRuleDetailsEntry(fixture, "User C", 10, 12, 1);

            new List<UserRuleDetailsDto>
                {
                    userRuleDetailsA,
                    userRuleDetailsB,
                    userRuleDetailsC,
                }
                .OrderByTotals()
                .ToArray()
                .Should()
                .Equal([
                    userRuleDetailsC,
                    userRuleDetailsB,
                    userRuleDetailsA,
                ]);
        }

        [Test]
        public void OrderByTotals_SortsByHighestOksThird()
        {
            var fixture = CreateFixture();

            var userRuleDetailsA = CreateUserRuleDetailsEntry(fixture, "User A", 10, 5, 3);
            var userRuleDetailsB = CreateUserRuleDetailsEntry(fixture, "User B", 10, 5, 1);
            var userRuleDetailsC = CreateUserRuleDetailsEntry(fixture, "User C", 10, 5, 2);

            new List<UserRuleDetailsDto>
                {
                    userRuleDetailsA,
                    userRuleDetailsB,
                    userRuleDetailsC,
                }
                .OrderByTotals()
                .ToArray()
                .Should()
                .Equal([
                    userRuleDetailsA,
                    userRuleDetailsC,
                    userRuleDetailsB,
                ]);
        }

        [Test]
        public void OrderByTotals_SortsByUserNameFourths()
        {
            var fixture = CreateFixture();

            var userRuleDetailsA = CreateUserRuleDetailsEntry(fixture, "User A", 10, 5, 3);
            var userRuleDetailsB = CreateUserRuleDetailsEntry(fixture, "User B", 10, 5, 3);
            var userRuleDetailsC = CreateUserRuleDetailsEntry(fixture, "User C", 10, 5, 3);

            new List<UserRuleDetailsDto>
                {
                    userRuleDetailsA,
                    userRuleDetailsB,
                    userRuleDetailsC,
                }
                .OrderByTotals()
                .ToArray()
                .Should()
                .Equal([
                    userRuleDetailsA,
                    userRuleDetailsB,
                    userRuleDetailsC,
                ]);
        }
    }

    [TestFixture]
    public class GetUserByIdSortingTests
    {
        [Test]
        public void OrderByName_SortsByUserName()
        {
            var fixture = CreateFixture();

            var getUserByIdEntryA = CreateGetUserByIdEntry(fixture, "User A");
            var getUserByIdEntryB = CreateGetUserByIdEntry(fixture, "User B");
            var getUserByIdEntryC = CreateGetUserByIdEntry(fixture, "User C");

            new List<GetUserById>
                {
                    getUserByIdEntryB,
                    getUserByIdEntryA,
                    getUserByIdEntryC,
                }
                .OrderByName()
                .ToArray()
                .Should()
                .Equal([
                    getUserByIdEntryA,
                    getUserByIdEntryB,
                    getUserByIdEntryC,
                ]);
        }
    }

    [TestFixture]
    public class GetPatByIdSortingTests
    {
        [Test]
        public void OrderByName_SortsByHighestCriticalFirst()
        {
            var fixture = CreateFixture();

            var getPatByIdEntryA = CreateGetPatByIdEntry(fixture, 0, 1, 1);
            var getPatByIdEntryB = CreateGetPatByIdEntry(fixture, 2, 1, 1);
            var getPatByIdEntryC = CreateGetPatByIdEntry(fixture, 1, 1, 1);

            new List<GetPatById>
                {
                    getPatByIdEntryA,
                    getPatByIdEntryB,
                    getPatByIdEntryC,
                }
                .OrderByTotals()
                .ToArray()
                .Should()
                .Equal([
                    getPatByIdEntryB,
                    getPatByIdEntryC,
                    getPatByIdEntryA,
                ]);
        }

        [Test]
        public void OrderByName_SortsByHighestWarningsSecond()
        {
            var fixture = CreateFixture();

            var getPatByIdEntryA = CreateGetPatByIdEntry(fixture, 1, 2, 1);
            var getPatByIdEntryB = CreateGetPatByIdEntry(fixture, 1, 1, 1);
            var getPatByIdEntryC = CreateGetPatByIdEntry(fixture, 1, 3, 1);

            new List<GetPatById>
                {
                    getPatByIdEntryA,
                    getPatByIdEntryB,
                    getPatByIdEntryC,
                }
                .OrderByTotals()
                .ToArray()
                .Should()
                .Equal([
                    getPatByIdEntryC,
                    getPatByIdEntryA,
                    getPatByIdEntryB,
                ]);
        }

        [Test]
        public void OrderByName_SortsByHighestOksThird()
        {
            var fixture = CreateFixture();

            var getPatByIdEntryA = CreateGetPatByIdEntry(fixture, 1, 1, 1);
            var getPatByIdEntryB = CreateGetPatByIdEntry(fixture, 1, 1, 3);
            var getPatByIdEntryC = CreateGetPatByIdEntry(fixture, 1, 1, 2);

            new List<GetPatById>
                {
                    getPatByIdEntryA,
                    getPatByIdEntryB,
                    getPatByIdEntryC,
                }
                .OrderByTotals()
                .ToArray()
                .Should()
                .Equal([
                    getPatByIdEntryB,
                    getPatByIdEntryC,
                    getPatByIdEntryA,
                ]);
        }
    }

    [TestFixture]
    public class RuleDtoSortingTests
    {
        [Test]
        public void OrderByName_SortsByRuleName()
        {
            var fixture = CreateFixture();

            var ruleDtoEntryA = CreateRuleDtoEntry(fixture, "Rule A");
            var ruleDtoEntryB = CreateRuleDtoEntry(fixture, "Rule B");
            var ruleDtoEntryC = CreateRuleDtoEntry(fixture, "Rule C");

            new List<RuleDto>
                {
                    ruleDtoEntryB,
                    ruleDtoEntryA,
                    ruleDtoEntryC,
                }
                .OrderByName()
                .ToArray()
                .Should()
                .Equal([
                    ruleDtoEntryA,
                    ruleDtoEntryB,
                    ruleDtoEntryC,
                ]);
        }
    }

    private static UserEntryDto CreateUserEntry(IFixture fixture, string userName, int totalUserCriticals, int totalUserWarnings, int totalUserOks)
    {
        return fixture
            .Build<UserEntryDto>()
            .OmitAutoProperties()
            .With(x => x.UserName, userName)
            .With(x => x.AffectedUserRulesCounts, CreateTotalSeverityCountsDto(fixture, totalUserCriticals, totalUserWarnings, totalUserOks))
            .Create();
    }

    private static GetUserById CreateGetUserByIdEntry(IFixture fixture, string userName)
    {
        return fixture
            .Build<GetUserById>()
            .OmitAutoProperties()
            .With(x => x.User, CreateUserDetailsDto(fixture, userName))
            .Create();
    }

    private static UserDetailsDto CreateUserDetailsDto(IFixture fixture, string userName)
    {
        return fixture
            .Build<UserDetailsDto>()
            .OmitAutoProperties()
            .With(x => x.UserName, userName)
            .Create();
    }

    private static GetPatById CreateGetPatByIdEntry(IFixture fixture, int totalCriticals, int totalWarnings, int totalOks)
    {
        return fixture
            .Build<GetPatById>()
            .OmitAutoProperties()
            .With(x => x.Pat, CreatePatDetailsEntry(fixture, fixture.Create<string>(), totalCriticals, totalWarnings, totalOks))
            .Create();
    }

    private static UserRuleDetailsDto CreateUserRuleDetailsEntry(IFixture fixture, string userName, int totalUserRulesCriticals, int totalUserRulesWarnings, int totalUserRulesOks)
    {
        return fixture
            .Build<UserRuleDetailsDto>()
            .OmitAutoProperties()
            .With(x => x.UserName, userName)
            .With(x => x.AffectedUserRulesCounts, CreateTotalSeverityCountsDto(fixture, totalUserRulesCriticals, totalUserRulesWarnings, totalUserRulesOks))
            .Create();
    }

    private static PatDetailsDto CreatePatDetailsEntry(IFixture fixture, string patName, int totalPatRulesCriticals, int totalPatRulesWarnings, int totalPatRulesOks)
    {
        return fixture
            .Build<PatDetailsDto>()
            .OmitAutoProperties()
            .With(x => x.Name, patName)
            .With(x => x.AffectedPatRulesCounts, CreateTotalSeverityCountsDto(fixture, totalPatRulesCriticals, totalPatRulesWarnings, totalPatRulesOks))
            .Create();
    }

    private static RuleDto CreateRuleDtoEntry(IFixture fixture, string ruleName, Categories category = Categories.Pats, Severities severity = Severities.Ok, int totalCounts = 0)
    {
        return fixture
            .Build<RuleDto>()
            .OmitAutoProperties()
            .With(x => x.Name, ruleName)
            .With(x => x.Scope, category)
            .With(x => x.Severity, severity)
            .With(x => x.TotalCounts, totalCounts)
            .Create();
    }

    private static TotalSeverityCountsDto CreateTotalSeverityCountsDto(IFixture fixture, int totalPatRulesCriticals, int totalPatRulesWarnings, int totalPatRulesOks)
    {
        return fixture
            .Build<TotalSeverityCountsDto>()
            .With(x => x.TotalCriticals, totalPatRulesCriticals)
            .With(x => x.TotalWarnings, totalPatRulesWarnings)
            .With(x => x.TotalOks, totalPatRulesOks)
            .Create();
    }

    private static Fixture CreateFixture()
    {
        return new Fixture();
    }
}