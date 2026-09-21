<script setup lang="ts">
import ChatForm from './components/ChatForm.vue'
</script>

<template>
  <div class="page">
    <main class="shell">
      <header class="hero">
        <span class="eyebrow">SVoy Electronics</span>

        <h1>Support Assistant</h1>
        
      </header>

      <ChatForm />
    </main>
  </div>
</template>

<style>
:root {
  font-family:
    Inter,
    ui-sans-serif,
    system-ui,
    -apple-system,
    BlinkMacSystemFont,
    "Segoe UI",
    sans-serif;

  color: #111827;
  background: #f3f4f6;
}

* {
  box-sizing: border-box;
}

body {
  margin: 0;
  min-width: 320px;
  min-height: 100vh;
}

button,
textarea {
  font: inherit;
}

.page {
  min-height: 100vh;
  padding: 48px 20px;
}

.shell {
  width: 100%;
  max-width: 820px;
  margin: 0 auto;
}

.hero {
  margin-bottom: 28px;
}

.eyebrow {
  display: inline-block;
  margin-bottom: 8px;
  font-size: 13px;
  font-weight: 700;
  letter-spacing: 0.08em;
  text-transform: uppercase;
  color: #4f46e5;
}

.hero h1 {
  margin: 0;
  font-size: clamp(36px, 6vw, 54px);
  line-height: 1.05;
  letter-spacing: -0.03em;
}

.hero p {
  max-width: 620px;
  margin: 14px 0 0;
  font-size: 18px;
  line-height: 1.6;
  color: #6b7280;
}
</style>