import { ref } from 'vue'
import { sendChatMessage } from '../services/chatService'
import type { ChatResponse } from '../models/chat'

export function useChat() {
  const result = ref<ChatResponse | null>(null)
  const error = ref<string | null>(null)
  const isLoading = ref(false)

  async function send(question: string) {
    error.value = null
    result.value = null
    isLoading.value = true

    try {
      result.value = await sendChatMessage({ question })
    } catch (exception) {
      error.value =
        exception instanceof Error
          ? exception.message
          : 'An unexpected error occurred.'
    } finally {
      isLoading.value = false
    }
  }

  return {
    result,
    error,
    isLoading,
    send,
  }
}