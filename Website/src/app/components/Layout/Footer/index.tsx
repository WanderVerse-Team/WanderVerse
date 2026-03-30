'use client'

import Link from 'next/link'
import { FooterLinksData } from '@/app/data/siteData'
import { motion } from 'framer-motion'

const SOCIALS = [
  { label: 'Facebook', href: 'https://facebook.com', icon: 'f' },
  { label: 'Twitter', href: 'https://twitter.com', icon: '𝕏' },
  { label: 'Instagram', href: 'https://instagram.com', icon: '◈' },
]

const footer = () => {
  const footerlinks = FooterLinksData

  return (
    <footer
      className="relative overflow-hidden"
      style={{
        background: 'linear-gradient(160deg, #0a0a1a 0%, #1a0a0a 50%, #0a0a1a 100%)',
      }}
      id="first-section"
    >
      {/* Background orbs */}
      <div
        className="absolute top-0 left-1/4 w-96 h-96 rounded-full opacity-10 animate-morph"
        style={{ background: 'radial-gradient(circle, #f35b03, transparent)', filter: 'blur(80px)' }}
      />
      <div
        className="absolute bottom-0 right-1/4 w-96 h-96 rounded-full opacity-10 animate-morph"
        style={{
          background: 'radial-gradient(circle, #7678ed, transparent)',
          filter: 'blur(80px)',
          animationDelay: '5s',
        }}
      />

      {/* Grid pattern */}
      <div
        className="absolute inset-0 opacity-5"
        style={{
          backgroundImage:
            'linear-gradient(rgba(255,255,255,0.1) 1px, transparent 1px), linear-gradient(90deg, rgba(255,255,255,0.1) 1px, transparent 1px)',
          backgroundSize: '60px 60px',
        }}
      />

      <div className="relative z-10 container mx-auto max-w-7xl px-6 pt-20 pb-10">
        {/* Top section */}
        <div className="grid grid-cols-1 lg:grid-cols-12 gap-12 mb-16">
          {/* Brand column */}
          <div className="lg:col-span-5">
            {/* Logo */}
            <div className="flex items-center gap-3 mb-6">
              <div
                className="w-10 h-10 rounded-2xl flex items-center justify-center text-white font-black text-lg shadow-lg"
                style={{ background: 'linear-gradient(135deg, #f35b03, #7678ed)' }}
              >
                W
              </div>
              <span
                className="text-2xl font-black"
                style={{
                  background: 'linear-gradient(135deg, #f35b03, #7678ed)',
                  WebkitBackgroundClip: 'text',
                  WebkitTextFillColor: 'transparent',
                  backgroundClip: 'text',
                }}
              >
                WanderVerse
              </span>
            </div>

            <p className="text-white/50 text-base leading-relaxed max-w-xs mb-8">
              Gamifying the local school syllabus — bridging education, trends, and
              technology for School Students across Sri Lanka.
            </p>

            {/* Social icons */}
            <div className="flex gap-3">
              {SOCIALS.map((s) => (
                <Link key={s.label} href={s.href} target="_blank" rel="noopener noreferrer">
                  <motion.div
                    whileHover={{ scale: 1.15, y: -3 }}
                    whileTap={{ scale: 0.95 }}
                    className="w-10 h-10 rounded-xl flex items-center justify-center text-sm font-bold text-white transition-colors duration-300"
                    style={{ background: 'rgba(255,255,255,0.08)', border: '1px solid rgba(255,255,255,0.1)' }}
                    title={s.label}
                  >
                    {s.icon}
                  </motion.div>
                </Link>
              ))}
            </div>
          </div>

          {/* Links columns */}
          <div className="lg:col-span-7 grid grid-cols-2 sm:grid-cols-3 gap-8">
            {footerlinks.map((section, i) => (
              <div key={i}>
                <p className="text-white font-black text-sm tracking-widest uppercase mb-5">
                  {section.section}
                </p>
                <ul className="space-y-3">
                  {section.links.map((link, j) => (
                    <li key={j}>
                      <Link
                        href={link.href}
                        className="text-white/50 text-sm hover:text-white transition-colors duration-200 hover:translate-x-1 inline-flex items-center gap-1 group"
                      >
                        <span className="opacity-0 group-hover:opacity-100 transition-opacity text-primary text-xs">→</span>
                        {link.label}
                      </Link>
                    </li>
                  ))}
                </ul>
              </div>
            ))}

            {/* Contact column */}
            <div>
              <p className="text-white font-black text-sm tracking-widest uppercase mb-5">
                Contact
              </p>
              <ul className="space-y-3">
                <li>
                  <a
                    href="mailto:wanderverse.team@gmail.com"
                    className="text-white/50 text-sm hover:text-primary transition-colors duration-200"
                  >
                    wanderverse.team@gmail.com
                  </a>
                </li>
                <li className="text-white/50 text-sm">
                  Colombo, Sri Lanka 🇱🇰
                </li>
                <li className="text-white/50 text-sm">IIT / Univ. of Westminster</li>
              </ul>
            </div>
          </div>
        </div>

        {/* Gradient divider */}
        <div
          className="h-px w-full mb-8 opacity-30"
          style={{ background: 'linear-gradient(90deg, transparent, #f35b03, #7678ed, transparent)' }}
        />

        {/* Bottom row */}
        <div className="flex flex-col sm:flex-row items-center justify-between gap-4">
          <p className="text-white/30 text-sm">
            © 2026 WanderVerse Team. All rights reserved.
          </p>
          <div className="flex gap-6">
            <Link href="/" className="text-white/30 text-sm hover:text-white/60 transition-colors">
              Privacy Policy
            </Link>
            <Link href="/" className="text-white/30 text-sm hover:text-white/60 transition-colors">
              Terms & Conditions
            </Link>
          </div>
        </div>
      </div>
    </footer>
  )
}

export default footer
