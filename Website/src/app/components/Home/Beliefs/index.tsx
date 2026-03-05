'use client'
import React, { useRef, useState } from 'react'
import { motion, useInView } from 'framer-motion'

const OUTCOMES = [
  'Colorful graphics & live interaction',
  'Play "games" instead of "studying"',
  'Deep theory + cognitive skills',
  'Follows the local school syllabus',
]

/* Radial spotlight that follows the cursor */
const SpotlightCard = ({
  children,
  className,
  style,
}: {
  children: React.ReactNode
  className?: string
  style?: React.CSSProperties
}) => {
  const ref = useRef<HTMLDivElement>(null)
  const [pos, setPos] = useState({ x: 50, y: 50 })

  const onMove = (e: React.MouseEvent<HTMLDivElement>) => {
    const rect = ref.current!.getBoundingClientRect()
    setPos({
      x: ((e.clientX - rect.left) / rect.width) * 100,
      y: ((e.clientY - rect.top) / rect.height) * 100,
    })
  }

  return (
    <div
      ref={ref}
      onMouseMove={onMove}
      className={`relative overflow-hidden group ${className ?? ''}`}
      style={{
        ...style,
        ['--mouse-x' as string]: `${pos.x}%`,
        ['--mouse-y' as string]: `${pos.y}%`,
      }}
    >
      {/* spotlight layer */}
      <div
        className="absolute inset-0 z-0 opacity-0 group-hover:opacity-100 pointer-events-none transition-opacity duration-500"
        style={{
          background:
            'radial-gradient(400px circle at var(--mouse-x,50%) var(--mouse-y,50%), rgba(255,255,255,0.11), transparent 65%)',
        }}
      />
      {children}
    </div>
  )
}

const Beliefs = () => {
  const ref = useRef<HTMLDivElement>(null)
  const inView = useInView(ref, { once: true, margin: '-60px' })

  return (
    <section id="Game" className="bg-cover bg-center overflow-hidden">
      <div className="container mx-auto max-w-7xl px-4">
        {/* Section label */}
        <motion.div
          ref={ref}
          initial={{ opacity: 0, y: 20 }}
          animate={inView ? { opacity: 1, y: 0 } : {}}
          transition={{ duration: 0.6 }}
          className="text-center mb-12"
        >
          <div className="inline-flex items-center gap-2 py-2 px-5 rounded-full bg-purple/10 border border-purple/20 mb-4">
            <span className="w-2 h-2 rounded-full bg-purple animate-pulse" />
            <p className="text-purple text-sm font-bold tracking-widest uppercase">
              Core Concept &amp; Outcomes
            </p>
          </div>
          <h2 className="text-black">
            How{' '}
            <span className="gradient-text">WanderVerse</span>{' '}
            works.
          </h2>
        </motion.div>

        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 items-stretch">
          {/* CARD 1 — Gamification */}
          <motion.div
            initial={{ opacity: 0, x: -60 }}
            animate={inView ? { opacity: 1, x: 0 } : {}}
            transition={{ duration: 0.8, delay: 0.1, ease: [0.22, 1, 0.36, 1] }}
            className="flex"
          >
            <SpotlightCard
              className="card-3d rounded-3xl w-full h-full"
              style={{
                background: 'linear-gradient(135deg, #7678ed, #3d348b)',
                minHeight: '400px',
              }}
            >
              {/* Noise texture */}
              <div
                className="absolute inset-0 opacity-10 z-0"
                style={{
                  backgroundImage:
                    "url(\"data:image/svg+xml,%3Csvg viewBox='0 0 200 200' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='noiseFilter'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.65' numOctaves='3' stitchTiles='stitch'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23noiseFilter)'/%3E%3C/svg%3E\")",
                }}
              />
              {/* Animated blob */}
              <div
                className="absolute -bottom-20 -right-20 w-64 h-64 rounded-full animate-morph z-0"
                style={{ background: 'rgba(255,255,255,0.08)', filter: 'blur(30px)' }}
              />
              {/* Decorative emoji background */}
              <div className="absolute bottom-0 right-0 opacity-20 pointer-events-none select-none text-9xl z-0">
                🎮
              </div>

              <div className="relative z-10 pt-12 px-10 sm:px-16 pb-14">
                {/* Animated icon */}
                <motion.div
                  animate={{ rotate: [0, -8, 8, 0], scale: [1, 1.12, 1] }}
                  transition={{ duration: 4, repeat: Infinity, ease: 'easeInOut' }}
                  className="w-14 h-14 rounded-2xl bg-white/15 border border-white/20 flex items-center justify-center text-3xl mb-6 backdrop-blur-sm"
                >
                  🎯
                </motion.div>

                <p className="text-sm font-bold text-white/60 tracking-widest uppercase mb-3">
                  Core Concept
                </p>
                <h3 className="text-white text-4xl font-black mb-4 leading-tight">
                  Gamification{' '}
                  <span className="opacity-60">of every lesson.</span>
                </h3>
                <p className="text-white/80 text-lg leading-relaxed">
                  Dives deeper into each concept in every lesson in the local syllabus,
                  grabbing the expected learning outcomes and gamifying each learning
                  point through levels.
                </p>

                {/* Tags */}
                <div className="flex flex-wrap gap-2 mt-8">
                  {['Grade 3 Math', 'Sinhala Medium', 'Levelled Gameplay'].map((tag) => (
                    <motion.span
                      key={tag}
                      whileHover={{ scale: 1.06, y: -2 }}
                      className="px-3 py-1 text-xs font-semibold text-white bg-white/15 border border-white/20 rounded-full backdrop-blur-sm"
                    >
                      {tag}
                    </motion.span>
                  ))}
                </div>
              </div>
            </SpotlightCard>
          </motion.div>

          {/* CARD 2 — Outcomes */}
          <motion.div
            initial={{ opacity: 0, x: 60 }}
            animate={inView ? { opacity: 1, x: 0 } : {}}
            transition={{ duration: 0.8, delay: 0.25, ease: [0.22, 1, 0.36, 1] }}
            className="flex"
          >
            <SpotlightCard
              className="card-3d rounded-3xl w-full h-full"
              style={{
                background: 'linear-gradient(135deg, #f7b801, #f35b03)',
                minHeight: '400px',
              }}
            >
              {/* Noise */}
              <div
                className="absolute inset-0 opacity-10 z-0"
                style={{
                  backgroundImage:
                    "url(\"data:image/svg+xml,%3Csvg viewBox='0 0 200 200' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='noiseFilter'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.65' numOctaves='3' stitchTiles='stitch'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23noiseFilter)'/%3E%3C/svg%3E\")",
                }}
              />
              <div
                className="absolute -top-20 -left-20 w-64 h-64 rounded-full animate-morph z-0"
                style={{
                  background: 'rgba(255,255,255,0.12)',
                  filter: 'blur(30px)',
                  animationDelay: '4s',
                }}
              />
              <div className="absolute bottom-0 right-0 opacity-20 pointer-events-none select-none text-9xl z-0">
                🏆
              </div>

              <div className="relative z-10 pt-12 px-10 sm:px-16 pb-14">
                <motion.div
                  animate={{ rotate: [0, 12, -12, 0], scale: [1, 1.12, 1] }}
                  transition={{ duration: 5, repeat: Infinity, ease: 'easeInOut', delay: 1 }}
                  className="w-14 h-14 rounded-2xl bg-white/20 border border-white/30 flex items-center justify-center text-3xl mb-6 backdrop-blur-sm"
                >
                  ✨
                </motion.div>

                <p className="text-sm font-bold text-black/50 tracking-widest uppercase mb-3">
                  Outcome
                </p>
                <h3 className="text-black font-black text-4xl mb-4 leading-tight">
                  <span className="text-white">Effortless</span> and fun learning.
                </h3>

                <ul className="space-y-3 mt-4">
                  {OUTCOMES.map((item, i) => (
                    <motion.li
                      key={i}
                      initial={{ opacity: 0, x: 20 }}
                      animate={inView ? { opacity: 1, x: 0 } : {}}
                      transition={{ duration: 0.5, delay: 0.4 + i * 0.12 }}
                      whileHover={{ x: 5, transition: { duration: 0.2 } }}
                      className="flex items-start gap-3"
                    >
                      <div className="w-6 h-6 rounded-full bg-white/30 flex items-center justify-center flex-shrink-0 mt-0.5 text-sm font-bold text-black">
                        ✓
                      </div>
                      <span className="text-black/80 text-base leading-snug">{item}</span>
                    </motion.li>
                  ))}
                </ul>
              </div>
            </SpotlightCard>
          </motion.div>
        </div>
      </div>
    </section>
  )
}

export default Beliefs
