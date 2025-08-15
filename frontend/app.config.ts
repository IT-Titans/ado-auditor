export default defineAppConfig({
  ui: {
    colors: {
      neutral: "zinc",
    },
    button: {
      variants: {
        size: {
          xl2: {
            base: 'px-6 py-4 text-base gap-2 text-xl/6 font-semibold ',
            leadingIcon: 'size-8',
            leadingAvatarSize: 'xs',
            trailingIcon: 'size-6'
          }
        },
      },
    }
  },
});
