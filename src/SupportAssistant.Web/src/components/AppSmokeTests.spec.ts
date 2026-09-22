import { mount } from '@vue/test-utils'
import { describe, expect, it } from 'vitest'
import App from '../App.vue'

describe('App', () => {
  it('renders support assistant title', () => {
    const wrapper = mount(App)

    expect(wrapper.get('header .eyebrow').text()).toBe('SVoy Electronics')
    expect(wrapper.get('h1').text()).toBe('Support Assistant')
  })
})
