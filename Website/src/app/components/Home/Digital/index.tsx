'use client'
import React, { useRef } from 'react'
import Image from 'next/image'
import { motion, useInView } from 'framer-motion'

const FEATURES = [
  { icon: '🎮', label: 'Game-Based' },
  { icon: '📚', label: 'Curriculum Aligned' },
  { icon: '🏆', label: 'Achievement System' },
  { icon: '🌟', label: 'Colorful Worlds' },
  { icon: '🧠', label: 'Cognitive Skills' },
  { icon: '📱', label: 'Mobile First' },
  { icon: '🎯', label: 'Grade 3 Math' },
  { icon: '🇱🇰', label: 'Sinhala Medium' },
]

// Duplicated for infinite loop illusion
const TICKER_ITEMS = [...FEATURES, ...FEATURES]

const Digital = () => {
  const ref = useRef<HTMLDivElement>(null)
  const inView = useInView(ref, { once: true, margin: '-80px' })

  return (
    <section className="relative bg-cover bg-center overflow-visible py-0">
      <div className="container mx-auto max-w-7xl px-4">
        <motion.div
          ref={ref}
          initial={{ opacity: 0, y: 60 }}
          animate={inView ? { opacity: 1, y: 0 } : {}}
          transition={{ duration: 0.9, ease: [0.22, 1, 0.36, 1] }}
          className="rounded-3xl overflow-hidden relative"
          style={{
            background: 'linear-gradient(135deg, #f35b03 0%, #3d348b 50%, #02398a 100%)',
          }}
        >
          {/* Animated gradient overlay */}
          <div
            className="absolute inset-0 animate-gradient opacity-30"
            style={{
              background:
                'linear-gradient(270deg, #7678ed, #f35b03, #f7b801, #7678ed)',
              backgroundSize: '400% 400%',
            }}
          />

          {/* Grid pattern */}
          <div
            className="absolute inset-0 opacity-10"
            style={{
              backgroundImage:
                'linear-gradient(rgba(255,255,255,0.1) 1px, transparent 1px), linear-gradient(90deg, rgba(255,255,255,0.1) 1px, transparent 1px)',
              backgroundSize: '40px 40px',
            }}
          />

          {/* Glowing orbs */}
          <div
            className="absolute -top-20 -right-20 w-64 h-64 rounded-full opacity-30 animate-morph"
            style={{ background: 'rgba(255,255,255,0.15)', filter: 'blur(40px)' }}
          />
          <div
            className="absolute -bottom-10 -left-10 w-48 h-48 rounded-full opacity-20 animate-morph"
            style={{
              background: 'rgba(247, 184, 1, 0.5)',
              filter: 'blur(30px)',
              animationDelay: '3s',
            }}
          />

          <div className="relative z-10 grid grid-cols-1 lg:grid-cols-2 py-16 lg:py-24 px-10 lg:px-24 gap-12 items-center">
            {/* LEFT */}
            <div>
              <motion.div
                initial={{ opacity: 0, x: -40 }}
                animate={inView ? { opacity: 1, x: 0 } : {}}
                transition={{ duration: 0.7, delay: 0.2 }}
              >
                <div className="inline-flex items-center gap-2 mb-6 py-1.5 px-4 rounded-full bg-white/10 border border-white/20">
                  <span className="w-2 h-2 rounded-full bg-yellow-400 animate-pulse" />
                  <span className="text-white/80 text-sm font-semibold tracking-widest uppercase">
                    WanderVerse
                  </span>
                </div>
                <h2 className="text-white mb-6 leading-tight text-4xl lg:text-5xl font-black">
                  Jump into the world
                  <br />
                  of{' '}
                  <span
                    style={{
                      background: 'linear-gradient(90deg, #f7b801, #fff)',
                      WebkitBackgroundClip: 'text',
                      WebkitTextFillColor: 'transparent',
                      backgroundClip: 'text',
                    }}
                  >
                    Willow
                  </span>{' '}
                  🦊
                </h2>
                <p className="text-white/70 text-lg leading-relaxed">
                  Where learning is a game and each level makes you smarter.
                  Gamification meets the local syllabus — making education
                  extraordinary.
                </p>
              </motion.div>

              {/* Stat pills */}
              <motion.div
                initial={{ opacity: 0, y: 20 }}
                animate={inView ? { opacity: 1, y: 0 } : {}}
                transition={{ duration: 0.6, delay: 0.5 }}
                className="flex flex-wrap gap-3 mt-8"
              >
                {[
                  { n: '10+', l: 'Levels' },
                  { n: '3rd', l: 'Grade' },
                  { n: '∞', l: 'Fun' },
                ].map((s) => (
                  <div
                    key={s.l}
                    className="flex flex-col items-center px-6 py-3 rounded-2xl bg-white/10 border border-white/20 backdrop-blur-sm"
                  >
                    <span className="text-2xl font-black text-white">{s.n}</span>
                    <span className="text-white/60 text-xs font-medium">{s.l}</span>
                  </div>
                ))}
              </motion.div>
            </div>

            {/* RIGHT — phone mockup */}
            <motion.div
              initial={{ opacity: 0, x: 40 }}
              animate={inView ? { opacity: 1, x: 0 } : {}}
              transition={{ duration: 0.8, delay: 0.3 }}
              className="relative flex justify-center"
            >
              <div className="relative">
                {/* Glow ring */}
                <div
                  className="absolute inset-0 m-auto rounded-full animate-pulse-glow"
                  style={{
                    width: '70%',
                    height: '70%',
                    top: '15%',
                    left: '15%',
                    background: 'rgba(255,255,255,0.1)',
                    filter: 'blur(20px)',
                  }}
                />
                <Image
                  src="/images/digital/girldoodle.svg"
                  alt="WanderVerse Character"
                  width={500}
                  height={500}
                  className="relative z-10 w-full max-w-md mx-auto animate-float"
                />
              </div>
            </motion.div>
          </div>
        </motion.div>
      </div>

      {/* Feature ticker below */}
      <div className="mt-10 py-5 overflow-hidden" style={{ background: 'rgba(0,0,0,0.03)' }}>
        <div className="ticker-wrap">
          <div className="ticker">
            {TICKER_ITEMS.map((item, i) => (
              <div
                key={i}
                className="inline-flex items-center gap-2 mx-8 text-black/60 font-semibold text-sm whitespace-nowrap"
              >
                <span className="text-2xl">{item.icon}</span>
                <span>{item.label}</span>
                <span className="ml-8 text-primary">✦</span>
              </div>
            ))}
          </div>
        </div>
      </div>
    </section>
  )
}

export default Digital
