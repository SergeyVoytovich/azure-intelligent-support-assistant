import type { ChatRequest, ChatResponse } from '../models/chat'

const apiBaseUrl =
  import.meta.env.VITE_API_BASE_URL ?? ''

export async function sendChatMessage(
  request: ChatRequest
): Promise<ChatResponse> {
  const response = await fetch(`${apiBaseUrl}/api/chat`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(request)
  })

  if (!response.ok) {
    const errorText = await response.text()

    throw new Error(
      `Chat request failed with status ${response.status}: ${errorText}`
    )
  }

  return await response.json() as ChatResponse
}