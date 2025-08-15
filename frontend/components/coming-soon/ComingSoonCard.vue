<script setup lang="ts">
interface Props {
  featureTitle?: string;
  featureSubtitle?: string;
  featureDescription?: string;
  icon?: string;
  status?: string;
  progress?: number;
  showProgress?: boolean;
  estimatedRelease?: string;
  showEstimatedRelease?: boolean;
  showNotificationSignup?: boolean;
  showComingSoonBadge?: boolean;
}

const {
  status = "Coming Soon",
  progress = 0,
  showProgress = false,
  estimatedRelease = "",
  showNotificationSignup = false,
  showEstimatedRelease = false,
  showComingSoonBadge = false,
  featureTitle = "Feature",
  featureSubtitle = "",
  featureDescription = "This feature is on our roadmap",
  icon = "i-lucide-settings",
} = defineProps<Props>();

// Notification signup state
const emailInput = ref("");
const isSubscribing = ref(false);
const isSubscribed = ref(false);
const subscriptionMessage = ref<{
  type: "success" | "error" | "warning" | "info";
  title: string;
  description: string;
} | null>(null);

// Computed properties
const isValidEmail = computed(() => {
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return emailRegex.test(emailInput.value);
});

// Methods
const handleNotificationSignup = async () => {
  if (!isValidEmail.value) return;

  isSubscribing.value = true;
  subscriptionMessage.value = null;

  try {
    // Simulate API call
    await new Promise((resolve) => setTimeout(resolve, 1500));

    // Simulate success
    isSubscribed.value = true;
    subscriptionMessage.value = {
      type: "success",
      title: "Successfully subscribed!",
      description: "You'll receive an email when this feature is available.",
    };
  } catch (error) {
    console.error(error);
    subscriptionMessage.value = {
      type: "error",
      title: "Subscription failed",
      description: "Please try again later or contact support.",
    };
  } finally {
    isSubscribing.value = false;
  }
};

// Accessibility
defineOptions({
  name: "FeaturePlaceholder",
});
</script>

<template>
  <UCard class="w-full max-w-lg mx-auto">
    <!-- Header Section -->
    <template #header>
      <div class="flex items-center justify-between">
        <div class="flex items-center space-x-3">
          <!-- Feature Icon Skeleton -->
          <div class="w-10 h-10 rounded-full flex items-center justify-center">
            <UIcon
              :name="icon"
              class="w-5 h-5 text-gray-400 dark:text-gray-500"
            />
          </div>

          <div class="space-y-2">
            <!-- Title Skeleton -->
            <div>{{ featureTitle }}</div>
            <!-- Subtitle Skeleton -->
            <div v-if="featureSubtitle" class="rounded">
              {{ featureSubtitle }}
            </div>
          </div>
        </div>

        <!-- Coming Soon Badge -->
        <ComingSoonBadge v-show="showComingSoonBadge" :status="status" />
      </div>
    </template>

    <!-- Content Section -->
    <div class="flex flex-col p-4 gap-y-4">
      <!-- Feature Preview -->
      <div class="bg-gray-50 dark:bg-gray-700/50 rounded-lg p-4">
        <Lottie name="adoWebsiteBuilding" />
      </div>
      <!-- Description Placeholder -->
      <div class="">
        <div class="w-full rounded">{{ featureDescription }}</div>
      </div>

      <!-- Progress Indicator -->
      <div v-show="showProgress" class="mt-8">
        <div class="flex justify-between items-center">
          <span class="text-sm text-gray-600 dark:text-gray-400"
            >Development Progress</span
          >
          <span class="text-sm font-medium text-gray-900 dark:text-gray-100"
            >{{ progress }}%</span
          >
        </div>

        <div class="w-full bg-gray-200 dark:bg-gray-700 rounded-full h-2">
          <div
            class="bg-blue-500 dark:bg-blue-600 h-2 rounded-full transition-all duration-500 ease-out"
            :style="{ width: `${progress}%` }"
          />
        </div>
      </div>

      <!-- Notification Signup -->
      <div v-if="showNotificationSignup" class="space-y-3">
        <div class="flex items-center space-x-2">
          <UIcon name="i-heroicons-bell" class="w-4 h-4 text-gray-500" />
          <span class="text-sm text-gray-700 dark:text-gray-300"
            >Get notified when this feature is ready</span
          >
        </div>

        <div class="flex space-x-2">
          <UInput
            v-model="emailInput"
            placeholder="Enter your email"
            :disabled="isSubscribed"
            class="flex-1"
            :ui="{ base: 'transition-all duration-200' }"
          />
          <UButton
            :loading="isSubscribing"
            :disabled="!isValidEmail || isSubscribed"
            color="primary"
            class="disabled-shimmer"
            @click="handleNotificationSignup"
          >
            {{ isSubscribed ? "Subscribed" : "Notify Me" }}
          </UButton>
        </div>

        <UAlert
          v-if="subscriptionMessage"
          :color="subscriptionMessage.type"
          :title="subscriptionMessage.title"
          :description="subscriptionMessage.description"
          class="mt-3"
        />
      </div>

      <div class="flex justify-center">
        <UButton
          size="xl"
          variant="outline"
          class="disabled-shimmer"
          aria-label="Preview feature (disabled)"
          @click="$router.back"
        >
          <UIcon name="i-lucide-arrow-left" class="w-4 h-4 mr-1" />
          Go Back
        </UButton>
      </div>
    </div>

    <!-- Footer Section -->
    <template v-if="showEstimatedRelease" #footer>
      <div class="flex items-center justify-center">
        <!--<div class="flex items-center space-x-4">-->
        <!-- Disabled Action Buttons -->
        <!--<UButton
            disabled
            size="xl"
            variant="outline"
            class="disabled-shimmer"
            aria-label="Preview feature (disabled)"
          >
            <UIcon name="i-heroicons-eye" class="w-4 h-4 mr-1" />
            Preview
          </UButton>

          <UButton
            disabled
            size="sm"
            variant="ghost"
            class="disabled-shimmer"
            aria-label="Configure feature (disabled)"
          >
            <UIcon name="i-heroicons-cog-6-tooth" class="w-4 h-4 mr-1" />
            Configure
          </UButton>-->
        <!--</div>-->

        <!-- Estimated Release -->
        <div
          v-show="showEstimatedRelease"
          class="text-xs text-gray-500 dark:text-gray-400"
        >
          <UIcon name="i-heroicons-calendar-days" class="w-3 h-3 mr-1 inline" />
          Est. {{ estimatedRelease }}
        </div>
      </div>
    </template>
  </UCard>
</template>

<style scoped>
/* Component-specific styles */
.pulse-glow {
  animation: pulse-glow 2s infinite;
}

/* Responsive adjustments */
@media (max-width: 640px) {
  .grid-cols-2 {
    grid-template-columns: 1fr;
  }
}

/* Focus states for accessibility */
.focus\:ring-2:focus {
  outline: 2px solid transparent;
  outline-offset: 2px;
  box-shadow: 0 0 0 2px rgba(59, 130, 246, 0.5);
}

/* High contrast mode support */
@media (prefers-contrast: high) {
  .skeleton {
    background: linear-gradient(90deg, #000 25%, #333 50%, #000 75%);
  }
}

/* Reduced motion support */
@media (prefers-reduced-motion: reduce) {
  .skeleton,
  .pulse-glow,
  .disabled-shimmer::after {
    animation: none;
  }
}
</style>
