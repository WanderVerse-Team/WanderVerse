import React from 'react'
import Hero from '@/app/components/Home/Hero'
import HorizontalScroll from '@/app/components/Home/HorizontalScroll'
import Aboutus from '@/app/components/Home/AboutUs'
import Beliefs from '@/app/components/Home/Beliefs'
import Work from '@/app/components/Home/Work'
import FAQ from '@/app/components/Home/FAQ'
import { Metadata } from 'next'

export const metadata: Metadata = {
  title: 'WanderVerse — Where Learning Meets Play',
  description:
    'WanderVerse gamifies the Grade 3 Sri Lankan Mathematics syllabus into an immersive adventure. Free to download.',
}

export default function Home() {
  return (
    <main>
      <Hero />
      <HorizontalScroll />
      <Beliefs />
      <Aboutus />
      <Work />
      <FAQ />
    </main>
  )
}
