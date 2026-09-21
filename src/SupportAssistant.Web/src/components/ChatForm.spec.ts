import { mount } from '@vue/test-utils'
import { describe, expect, it } from 'vitest'
import ChatForm from './ChatForm.vue'

describe('ChatForm', () => {
  it('shows validation error for empty question', async () => {
    const wrapper = mount(ChatForm)

    await wrapper.find('form').trigger('submit')

    expect(wrapper.text())
      .toContain('Please enter a question.')
  })
})