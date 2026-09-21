<script setup lang="ts">
import { ref } from 'vue'
import { sendChatMessage } from '../services/chatService'
import type { ChatResponse } from '../models/chat'

const question = ref('')
const result = ref<ChatResponse | null>(null)
const error = ref<string | null>(null)
const isLoading = ref(false)

async function submit(): Promise<void> {
  const trimmedQuestion = question.value.trim()

  if (!trimmedQuestion) {
    error.value = 'Please enter a question.'
    return
  }

  isLoading.value = true
  error.value = null
  result.value = null

  try {
    result.value = await sendChatMessage({
      question: trimmedQuestion
    })
  } catch (exception) {
    error.value =
      exception instanceof Error
        ? exception.message
        : 'Unexpected error.'
  } finally {
    isLoading.value = false
  }
}
</script>

<template>
  <section class="chat-card">
    <form
      class="chat-form"
      @submit.prevent="submit"
    >
      <label
        class="label"
        for="question"
      >
        Your question
      </label>

      <textarea
        id="question"
        v-model="question"
        class="textarea"
        rows="6"
        maxlength="4000"
        placeholder="For example: How long is the warranty?"
      />

      <div class="actions">
        <span class="counter">
          {{ question.length }} / 4000
        </span>

        <button
          class="submit-button"
          type="submit"
          :disabled="isLoading"
        >
          {{ isLoading ? 'Sending…' : 'Send question' }}
        </button>
      </div>
    </form>

    <div
      v-if="error"
      class="message error"
      role="alert"
    >
      {{ error }}
    </div>

    <article
      v-if="result"
      class="result"
    >
      <div class="result-header">
        <h2>Answer</h2>

        <span
          v-if="result.escalationRequired"
          class="badge warning"
        >
          Human support required
        </span>

        <span
          v-else
          class="badge success"
        >
          AI response
        </span>
      </div>

      <p class="answer">
        {{ result.answer }}
      </p>

      <section
        v-if="result.sources.length > 0"
        class="sources"
      >
        <h3>Sources</h3>

        <div class="source-list">
          <span
            v-for="source in result.sources"
            :key="source"
            class="source-chip"
          >
            {{ source }}
          </span>
        </div>
      </section>

      <div class="request-id">
        Request ID: {{ result.requestId }}
      </div>
    </article>
  </section>
</template>

<style scoped>
.chat-card {
  overflow: hidden;
  border: 1px solid #e5e7eb;
  border-radius: 18px;
  background: white;
  box-shadow:
    0 1px 2px rgba(0, 0, 0, 0.04),
    0 16px 40px rgba(17, 24, 39, 0.06);
}

.chat-form {
  padding: 24px;
}

.label {
  display: block;
  margin-bottom: 10px;
  font-size: 14px;
  font-weight: 700;
  color: #374151;
}

.textarea {
  width: 100%;
  resize: vertical;
  min-height: 150px;
  padding: 16px;
  border: 1px solid #d1d5db;
  border-radius: 12px;
  outline: none;
  background: #fafafa;
  color: #111827;
  line-height: 1.5;
  transition:
    border-color 0.15s ease,
    box-shadow 0.15s ease,
    background 0.15s ease;
}

.textarea:focus {
  border-color: #6366f1;
  background: white;
  box-shadow: 0 0 0 4px rgba(99, 102, 241, 0.12);
}

.actions {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  margin-top: 14px;
}

.counter {
  font-size: 12px;
  color: #9ca3af;
}

.submit-button {
  border: 0;
  border-radius: 10px;
  padding: 11px 18px;
  font-weight: 700;
  color: white;
  background: #4f46e5;
  cursor: pointer;
  transition:
    transform 0.15s ease,
    background 0.15s ease;
}

.submit-button:hover:not(:disabled) {
  background: #4338ca;
  transform: translateY(-1px);
}

.submit-button:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.message {
  margin: 0 24px 24px;
  padding: 12px 14px;
  border-radius: 10px;
  font-size: 14px;
}

.error {
  border: 1px solid #fecaca;
  background: #fef2f2;
  color: #991b1b;
}

.result {
  border-top: 1px solid #e5e7eb;
  padding: 24px;
  background: #fafafa;
}

.result-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  margin-bottom: 16px;
}

.result-header h2 {
  margin: 0;
  font-size: 20px;
}

.badge {
  padding: 5px 9px;
  border-radius: 999px;
  font-size: 12px;
  font-weight: 700;
}

.success {
  background: #ecfdf5;
  color: #047857;
}

.warning {
  background: #fff7ed;
  color: #c2410c;
}

.answer {
  margin: 0;
  white-space: pre-wrap;
  line-height: 1.7;
  color: #374151;
}

.sources {
  margin-top: 24px;
}

.sources h3 {
  margin: 0 0 10px;
  font-size: 14px;
}

.source-list {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.source-chip {
  padding: 6px 10px;
  border: 1px solid #d1d5db;
  border-radius: 999px;
  background: white;
  font-size: 12px;
  color: #4b5563;
}

.request-id {
  margin-top: 24px;
  font-size: 11px;
  color: #9ca3af;
  word-break: break-all;
}

@media (max-width: 600px) {
  .chat-form,
  .result {
    padding: 18px;
  }

  .actions,
  .result-header {
    align-items: stretch;
    flex-direction: column;
  }

  .submit-button {
    width: 100%;
  }
}
</style>