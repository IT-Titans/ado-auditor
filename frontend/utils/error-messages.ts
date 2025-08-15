export type ErrorContent = {
  title: string;
  message: string;
  icon?: string;
  iconClass?: string;
  suggestions?: Array<string>;
};

export type ErrorMassages = Record<number, ErrorContent>;

export const errorMassages: ErrorMassages = {
  400: {
    title: "Oops! Bad Request",
    message:
      "It looks like there was an issue with your request. Don't worry, it happens to the best of us!",
    icon: "i-lucide-triangle-alert",
    iconClass: "h-16 w-16 text-orange-500",
    suggestions: [
      "Double-check the information you entered",
      "Try refreshing the page",
      "Go back and try again",
    ],
  },
  401: {
    title: "Authentication Required",
    message:
      "You'll need to sign in to access this page. No worries, it only takes a moment!",
    icon: "i-lucide-lock",
    iconClass: "h-16 w-16 text-blue-500",
    suggestions: ["Sign in to your account", "Check if you're still logged in"],
  },
  403: {
    title: "Access Denied",
    message:
      "Sorry, you don't have permission to view this page. But there's plenty more to explore!",
    icon: "i-lucide-lock",
    iconClass: "h-16 w-16 text-blue-500",
    suggestions: [
      "Contact your admin to get access to this resource on Azure DevOps",
      "Check if you have the right permissions",
      "Try accessing a different section",
    ],
  },
  404: {
    title: "Page Not Found",
    message:
      "The page you're looking for seems to have wandered off. Let's get you back on track!",
    icon: "i-lucide-search",
    iconClass: "h-16 w-16 text-purple-500",
    suggestions: ["Check the URL for typos", "Browse our main sections"],
  },
  410: {
    title: "Content No Longer Available",
    message:
      "The page or resource you’re trying to access is no longer available on this server. It was intentionally removed and won’t be coming back.",
    icon: "i-lucide-search",
    iconClass: "h-16 w-16 text-purple-500",
    suggestions: ["Try refreshing the page", "Restart the process"],
  },
  500: {
    title: "Server Error",
    message:
      "Something went wrong on our end. Our team has been notified and we're working on it!",
    icon: "i-lucide-server",
    iconClass: "h-16 w-16 text-red-500",
    suggestions: [
      "Try refreshing the page in a few minutes",
      "Check our status page for updates",
      "Contact support if the issue persists",
    ],
  },
  502: {
    title: "Bad Gateway",
    message:
      "We're having trouble connecting to our servers right now. Please bear with us!",
    icon: "i-lucide-wifi",
    iconClass: "h-16 w-16 text-orange-500",
    suggestions: [
      "Wait a moment and try again",
      "Check your internet connection",
      "Try again in a few minutes",
    ],
  },
  503: {
    title: "Service Unavailable",
    message:
      "We're temporarily down for maintenance. We'll be back up and running soon!",
    icon: "i-lucide-server",
    iconClass: "h-16 w-16 text-yellow-500",
    suggestions: [
      "Check back in a few minutes",
      "Follow us on social media for updates",
      "Try again later",
    ],
  },
};

export function getErrorMessage(statusCode?: number): string {
  if (!statusCode) {
    return "An unknown error has occurred.";
  }

  return errorMassages[statusCode].message || "An unknown error has occurred.";
}

export function getErrorTitle(statusCode?: number): string {
  if (!statusCode) {
    return "Unknown error!";
  }
  return errorMassages[statusCode].title || "Unknown error!";
}