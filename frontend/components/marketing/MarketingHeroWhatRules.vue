<script setup lang="ts">
const rulesDummyData: Array<BaseRule> = [
  {
    "id": "2",
    "scope": "pats",
    "severity": "critical",
    "name": "PAT has app_token scope",
    "description": "Personal Access Tokens (PATs) with the app_token scope grant broad and powerful permissions intended for applications, often beyond what is necessary for typical user tasks. This elevated level of access increases the risk of misuse, especially if the token is leaked or used in insecure environments. Over-privileged tokens violate the principle of least privilege, making them a high-value target for attackers and a significant risk to the security of your Azure DevOps environment.",
    "recommendation": "To reduce risk, avoid using PATs with the app_token scope unless absolutely required. Review your existing tokens under User Settings > Personal Access Tokens, and revoke any that include the app_token scope unnecessarily. Instead, create new tokens with only the minimum scopes required for the specific task or workflow. If you need application-level access, consider using Azure AD service principals or OAuth-based authentication with scoped permissions for more secure and manageable access.",
    "totalCounts": 9
  },
  {
    "id": "4",
    "scope": "pats",
    "severity": "warning",
    "name": "PAT expires in over 90 days",
    "description": "Personal Access Tokens (PATs) with expiry dates longer than 90 days increase the risk of unauthorized access if they are ever leaked or compromised. Since these tokens often provide broad permissions, a long lifespan gives attackers more time to exploit them without detection. Shorter-lived tokens reduce this risk by limiting the duration of access and encouraging regular token rotation, which is a key security best practice.",
    "recommendation": "To improve security in your Azure DevOps Service instance, we recommend setting a shorter expiration period for Personal Access Tokens—ideally 90 days or less. You can do this by navigating to User Settings > Personal Access Tokens, revoking any long-lived tokens, and creating new ones with shorter expiry periods and the minimum required scopes.",
    "totalCounts": 7
  },
  {
    "id": "5",
    "scope": "pats",
    "severity": "ok",
    "name": "PAT has limited scope",
    "description": "Personal Access Tokens (PATs) with the minimum required scopes are a strong security practice because they follow the principle of least privilege—granting only the specific permissions needed for a task and nothing more. This limits the potential damage if a token is ever compromised, reduces the risk of accidental misuse, and makes it easier to audit and manage access. Using minimally scoped tokens helps maintain tighter control over your Azure DevOps environment and supports a more secure and compliant development workflow.",
    "recommendation": "You're following best practices. Nothing to do here!",
    "totalCounts": 31
  },
  {
    "id": "6",
    "scope": "pats",
    "severity": "ok",
    "name": "PAT expires in a short time",
    "description": "Using short-lived Personal Access Tokens (PATs) is a best practice because it limits the time a token can be used if it becomes compromised. By reducing the lifespan of tokens, you minimize the window of opportunity for unauthorized access, enforce more frequent access reviews, and encourage regular credential rotation. This approach strengthens your overall security posture by ensuring that access credentials are refreshed more often and less likely to be misused or forgotten.",
    "recommendation": "You're following best practices. Nothing to do here!",
    "totalCounts": 14
  }
];
</script>

<template>
  <SectionsHero class="grid grid-cols-1 gap-8 2xl:grid-cols-2">
      <div class="my-10 mx-10 order-last 2xl:my-20 2xl:ml-20 2xl:mr-0 2xl:order-first">
        <RulesTable class="shadow md:shadow-md/20 lg:shadow-xl/20 2xl:shadow-2xl/20" :data=rulesDummyData />
      </div>
      <div class="px-6 pt-8 2xl:pt-20 text-center">
        <UIcon name="i-lucide-glasses" class="text-7xl text-severity-warning"/>
        <h1 class="text-4xl text-center font-bold text-highlighted pb-4">See what rules</h1>
        <p class="mx-auto text-base/8 font-medium text-pretty text-toned sm:text-lg/8">
          We provide you with a clear overview of the rules that we checked against.<br >
          Get clear metrics about how often a rule matched so you get to know if your colleagues behave according to best practices.
        </p>
      </div>
  </SectionsHero>
</template>

<style scoped>

</style>