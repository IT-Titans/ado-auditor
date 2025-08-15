<script setup lang="ts">
const route = useRoute();
async function handleClick() {
  try {
    const response = await useAsyncData(() =>
      $fetch("/api/reports", {
        method: "GET",
        headers: {
          Accept: "application/pdf",
        },
        query: {
          auditId: route.params.id,
        },
        responseType: "blob",
        raw: true,
      })
    );

    const blob = await response.data.value;

    if (response.error.value) {
      throw createError({
        statusCode: 500,
        statusText: "Can't create report pdf.",
      });
    } else {
      const url = URL.createObjectURL(blob);
      const link = document.createElement("a");
      const currentDate = useDateFormat(useNow(), "YYYY_MM_DD_HH:mm");

      console.log(currentDate.value);
      link.href = url;
      link.download = `ado-audit-report_${currentDate.value}.pdf`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      URL.revokeObjectURL(url);
    }
  } catch (e) {
    console.error(e);
    throw e;
  }
}
</script>

<template>
  <button class="Btn" @click="handleClick">
    <svg
      viewBox="0 0 25 25"
      class="bi bi-twitter"
      fill="currentColor"
      height="30"
      width="30"
      xmlns="http://www.w3.org/2000/svg"
    >
      <path
        d="M5 20h14a1 1 0 0 0 1-1v-6h-2v6H6v-6H4v6a1 1 0 0 0 1 1zm7-16-5.5 5.5 1.42 1.42L11 8.83V16h2V8.83l3.08 3.09 1.42-1.42L12 4z"
      />
    </svg>
    <span>Export Audit</span>
  </button>
</template>

<style scoped>
button {
  background: transparent;
  position: relative;
  padding: 5px 15px;
  display: flex;
  align-items: center;
  font-size: 17px;
  font-weight: 600;
  text-decoration: none;
  cursor: pointer;
  border: 1px solid #741c97;
  border-radius: 25px;
  outline: none;
  overflow: hidden;
  color: #741c97;
  transition: color 0.3s 0.1s ease-out;
  text-align: center;
  height: 45px;
}

button span {
  margin: 10px;
}

button::before {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  margin: auto;
  content: "";
  border-radius: 50%;
  display: block;
  width: 20em;
  height: 20em;
  left: -5em;
  text-align: center;
  transition: box-shadow 0.4s ease-out;
  z-index: -1;
}

button:hover {
  color: #fff;
  border: #741c97;
}

button:hover::before {
  box-shadow: inset 0 0 0 10em #741c97;
}
</style>
