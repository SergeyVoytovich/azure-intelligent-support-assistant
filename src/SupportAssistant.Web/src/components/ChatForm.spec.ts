import { mount } from '@vue/test-utils'
import { describe, expect, it, vi } from 'vitest'
import ChatForm from './ChatForm.vue'
import * as chatService from '../services/chatService'

describe('ChatForm', () => {
  it('shows answer returned by API', async () => {
    vi.spyOn(chatService, 'sendChatMessage').mockResolvedValue({
      answer: 'The warranty is 24 months.',
      sources: ['warranty.pdf'],
      escalationRequired: false,
      requestId: 'req-123'
    })

    const wrapper = mount(ChatForm)

    await wrapper.find('textarea').setValue('How long is the warranty?')
    await wrapper.find('form').trigger('submit')

    await vi.waitFor(() => {
      expect(wrapper.text()).toContain('The warranty is 24 months.')
    })

    expect(wrapper.text()).toContain('warranty.pdf')
    expect(wrapper.text()).toContain('req-123')
    expect(wrapper.text()).toContain('AI response')
  })

  it('shows escalation state when human support is required', async () => {
  vi.spyOn(chatService, 'sendChatMessage').mockResolvedValue({
    answer: 'I will forward this request to human support.',
    sources: [],
    escalationRequired: true,
    requestId: 'req-456'
  })

  const wrapper = mount(ChatForm)

  await wrapper.find('textarea').setValue('I am very angry and need help.')
  await wrapper.find('form').trigger('submit')

  await vi.waitFor(() => {
    expect(wrapper.text()).toContain('Human support required')
  })

  expect(wrapper.text()).toContain('I will forward this request to human support.')
})

it('shows error when API request fails', async () => {
    vi.spyOn(chatService, 'sendChatMessage').mockRejectedValue(
      new Error('Backend unavailable')
    )

    const wrapper = mount(ChatForm)

    await wrapper.find('textarea').setValue('How long is the warranty?')
    await wrapper.find('form').trigger('submit')

    await vi.waitFor(() => {
      expect(wrapper.text()).toContain('Backend unavailable')
    })
  })
})