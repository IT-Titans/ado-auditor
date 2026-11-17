# Security Policy

ADO Auditor analyses Azure DevOps organisations and handles access tokens
belonging to our users. We take reports about it seriously.

## Reporting a vulnerability

**Please do not open a public issue.** Use one of these instead:

- GitHub private vulnerability reporting: the **Security** tab of this
  repository, then **Report a vulnerability**
- Email through https://it-titans-gmbh.com/kontakt/, with "Security" in the
  subject line

Useful things to include: what the problem is, how to reproduce it, which
version or commit you looked at, and what an attacker could achieve. A proof of
concept helps, but a clear description is enough to start.

## What happens next

| Step | Our commitment |
|---|---|
| Acknowledgement | within 5 working days |
| Initial assessment | within 15 working days |
| Progress updates | at least every 30 days while we are working on it |

We will tell you how we assess the report, what we intend to do, and when. If
we disagree that something is a vulnerability, we will say so and explain why.

## Scope

In scope is the code in this repository and the hosted service at
https://ado-auditor.com.

**Testing against the hosted service requires our prior written agreement.**
Please ask first and describe what you intend to do. Without an agreement,
please confine yourself to a local instance, which you can start from this
repository. We ask for this because the service processes data belonging to our
users, not because we are uninterested in the findings.

Out of scope: reports produced solely by an automated scanner without a
demonstrated impact, missing hardening headers with no exploitable
consequence, denial of service through volume alone, and social engineering
against our staff.

## Credit

Tell us how you would like to be credited and we will name you in the release
notes of the fix, or stay silent if you prefer.
