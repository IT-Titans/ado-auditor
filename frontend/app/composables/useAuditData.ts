export const useAuditData = async () => {
  const router = useRouter();
  const { params } = router.currentRoute.value;

  const state = useState<AuditData | null>(
    `audit-data-${params.id}`,
    () => null
  );

  if (!state.value) {
    const { data, error } = await useAsyncData<AuditData>(
      `audit-data-${params.id}`,
      () => {
        return $fetch<AuditData>(`/api/audit/${params.id}`, {
          timeout: 1000 * 60 * 10,
        });
      }
    );

    if (error.value) {
      console.error("Error fetching audit data: ", error.value);
      if (error.value.statusCode === 401 || error.value.statusCode === 410) {
        router.replace(linkTemplates.global.expiredAudit());
      }
    }

    data.value!.summaries[2] = {
      scope: "projects",
      criticalCount: 0,
      warningCount: 0,
      okCount: 0,
      totalCount: 0,
      items: [
        {
          severity: "critical",
          affectedCount: 0,
          totalCount: 0,
          text: "Projects with weak passwords",
        },
        {
          severity: "warning",
          affectedCount: 0,
          totalCount: 0,
          text: "Projects with outdated MFA",
        },
        {
          severity: "ok",
          affectedCount: 0,
          totalCount: 0,
          text: "Projects with strong security settings",
        },
      ],
    };

    data.value!.summaries[3] = {
      scope: "groups",
      criticalCount: 0,
      warningCount: 0,
      okCount: 0,
      totalCount: 0,
      items: [
        {
          severity: "critical",
          affectedCount: 0,
          totalCount: 0,
          text: "Users with weak passwords",
        },
        {
          severity: "warning",
          affectedCount: 0,
          totalCount: 0,
          text: "Users with outdated MFA",
        },
        {
          severity: "ok",
          affectedCount: 0,
          totalCount: 0,
          text: "Users with strong security settings",
        },
      ],
    };

    state.value = data.value!;
  }

  return state;
};
