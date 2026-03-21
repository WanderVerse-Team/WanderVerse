'use client'
import { useRef, useState } from 'react'
import { aboutdata } from '@/app/types/aboutdata'
import { Aboutdata } from '@/app/data/siteData'
import Image from 'next/image'
import { motion, useInView } from 'framer-motion'

const CARD_ACCENTS = [
  { gradient: 'from-orange-400 to-primary', shadow: 'rgba(243,91,3,0.25)', emoji: '🚀' },
  { gradient: 'from-purple-500 to-purple', shadow: 'rgba(118,120,237,0.25)', emoji: '🎯' },
  { gradient: 'from-yellow-400 to-orange-400', shadow: 'rgba(247,184,1,0.25)', emoji: '🌟' },
]

// Spotlight card component
const SpotCard = ({ children, className, style, shadow }: {
  children: React.ReactNode
  className?: string
  style?: React.CSSProperties
  shadow?: string
}) => {
  const ref = useRef<HTMLDivElement>(null)
  const [pos, setPos] = useState({ x: 50, y: 50 })
  const onMove = (e: React.MouseEvent<HTMLDivElement>) => {
    const r = ref.current!.getBoundingClientRect()
    setPos({ x: ((e.clientX - r.left) / r.width) * 100, y: ((e.clientY - r.top) / r.height) * 100 })
  }
  return (
    <div
      ref={ref}
      onMouseMove={onMove}
      className={`relative overflow-hidden group ${className ?? ''}`}
      style={{ ...style, ['--mouse-x' as string]: `${pos.x}%`, ['--mouse-y' as string]: `${pos.y}%` }}
    >
      <div
        className="absolute inset-0 z-0 pointer-events-none opacity-0 group-hover:opacity-100 transition-opacity duration-500"
        style={{ background: `radial-gradient(350px circle at var(--mouse-x,50%) var(--mouse-y,50%), ${shadow ?? 'rgba(243,91,3,0.06)'}, transparent 65%)` }}
      />
      {children}
    </div>
  )
}

const Aboutus = () => {
  const about = Aboutdata
  const ref = useRef<HTMLDivElement>(null)
  const inView = useInView(ref, { once: true, margin: '-80px' })

  return (
    <section id="About" className="overflow-hidden">
      <div className="container mx-auto max-w-7xl px-4 relative z-1">
        {/* Section header */}
        <motion.div
          ref={ref}
          initial={{ opacity: 0, y: 30 }}
          animate={inView ? { opacity: 1, y: 0 } : {}}
          transition={{ duration: 0.7 }}
          className="text-center mb-16"
        >
          <div className="inline-flex items-center gap-2 py-2 px-5 rounded-full bg-primary/10 border border-primary/20 mb-4">
            <span className="w-2 h-2 rounded-full bg-primary animate-pulse" />
            <p className="text-primary text-sm font-bold tracking-widest uppercase">
              About Us
            </p>
          </div>
          <h2 className="text-black">
            Get to know{' '}
            <span className="gradient-text">our team.</span>
          </h2>
          <p className="text-black/50 text-xl mt-4 max-w-xl mx-auto">
            A passionate group of Computer Science undergraduates building the future of education.
          </p>
        </motion.div>

        {/* Cards */}
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-8 items-stretch mt-8">
          {about.map((item, i) => {
            const accent = CARD_ACCENTS[i % CARD_ACCENTS.length]
            return (
              <motion.div
                key={i}
                initial={{ opacity: 0, y: 50 }}
                animate={inView ? { opacity: 1, y: 0 } : {}}
                transition={{ duration: 0.6, delay: i * 0.15, ease: 'easeOut' }}
                className="group/minimal relative flex flex-col h-full bg-white rounded-3xl border border-black/10 p-2 transition-all duration-500 hover:shadow-[0_20px_40px_-15px_rgba(0,0,0,0.1)] hover:-translate-y-2 overflow-hidden"
              >
                {/* Image Showcase Block */}
                <div className="relative w-full h-56 rounded-[20px] overflow-hidden bg-black/[0.03] flex items-center justify-center p-6 border border-black/[0.03]">
                  {/* Reveal gradient background on hover */}
                  <div className={`absolute inset-0 bg-gradient-to-br ${accent.gradient} opacity-0 group-hover/minimal:opacity-100 transition-opacity duration-700`} />

                  {/* Animated grain/noise on hover */}
                  <div
                    className="absolute inset-0 opacity-0 group-hover/minimal:opacity-[0.15] mix-blend-overlay transition-opacity duration-700 pointer-events-none"
                    style={{
                      backgroundImage: "url(\"data:image/svg+xml,%3Csvg viewBox='0 0 200 200' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='noiseFilter'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.85' numOctaves='3' stitchTiles='stitch'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23noiseFilter)'/%3E%3C/svg%3E\")"
                    }}
                  />

                  <motion.div
                    className="relative w-full h-full z-10"
                    whileHover={{ scale: 1.15, rotate: 3, y: -5 }}
                    transition={{ type: "spring", stiffness: 300, damping: 20 }}
                  >
                    <Image
                      src={item.imgSrc}
                      alt={item.heading}
                      fill
                      className="object-contain drop-shadow-sm group-hover/minimal:drop-shadow-2xl transition-all duration-500"
                    />
                  </motion.div>
                </div>

                {/* Content Block */}
                <div className="flex flex-col flex-grow p-6 pt-8 relative z-10 bg-white">
                  <div className="flex justify-between items-end mb-4 overflow-hidden">
                    <div>
                      <div className="flex items-center gap-2 mb-3 transform group-hover/minimal:translate-y-1 transition-transform duration-300">
                        <span className="text-xl bg-white rounded-full w-10 h-10 flex items-center justify-center shadow-sm border border-black/5 transform group-hover/minimal:-rotate-12 transition-transform duration-300">
                          {accent.emoji}
                        </span>
                        <span className={`text-[10px] font-black uppercase tracking-[0.2em] bg-clip-text text-transparent bg-gradient-to-r ${accent.gradient} opacity-0 group-hover/minimal:opacity-100 transform -translate-x-4 group-hover/minimal:translate-x-0 transition-all duration-300`}>
                          Interactive
                        </span>
                      </div>
                      <h3 className="text-3xl leading-none font-black text-black tracking-tight transform group-hover/minimal:translate-x-1 transition-transform duration-300">
                        {item.heading}
                      </h3>
                    </div>
                  </div>

                  <p className="text-black/50 text-base leading-relaxed mb-8 flex-grow group-hover/minimal:text-black/80 transition-colors duration-300">
                    {item.paragraph}
                  </p>

                  {/* Footer */}
                  <div className="mt-auto flex items-center justify-between border-t border-black/5 pt-5 group-hover/minimal:border-black/20 transition-colors duration-300">
                    <span className="text-sm font-bold tracking-widest uppercase text-black">
                      {item.link || "Discover"}
                    </span>
                    <div className="w-10 h-10 rounded-full border border-black text-black flex items-center justify-center group-hover/minimal:bg-black group-hover/minimal:text-white transition-all duration-300 transform group-hover/minimal:translate-x-1 relative overflow-hidden">
                      <div className={`absolute inset-0 bg-gradient-to-br ${accent.gradient} opacity-0 group-hover/minimal:opacity-100 transition-opacity duration-300`} />
                      <svg className="w-4 h-4 relative z-10" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2.5} d="M14 5l7 7m0 0l-7 7m7-7H3" />
                      </svg>
                    </div>
                  </div>
                </div>
              </motion.div>
            )
          })}
        </div>

        {/* Bottom CTA banner */}
        <motion.div
          initial={{ opacity: 0, y: 30 }}
          animate={inView ? { opacity: 1, y: 0 } : {}}
          transition={{ duration: 0.7, delay: 0.6 }}
          className="mt-16 relative rounded-3xl overflow-hidden py-12 px-10 text-center"
          style={{
            background: 'linear-gradient(135deg, #3d348b, #02398a)',
          }}
        >
          <div
            className="absolute inset-0 animate-gradient opacity-20"
            style={{
              background: 'linear-gradient(270deg, #f35b03, #7678ed, #f7b801)',
              backgroundSize: '400% 400%',
            }}
          />
          <div className="relative z-10">
            <p className="text-white/60 text-sm font-bold tracking-widest uppercase mb-3">
              WanderVerse Team
            </p>
            <h3 className="text-white text-3xl font-black mb-4">
              Built with ❤️ by IIT Undergrads
            </h3>
            <p className="text-white/60 max-w-lg mx-auto text-lg">
              University of Westminster affiliated — bridging school education,
              trends and technology since 2024.
            </p>
          </div>
        </motion.div>
      </div>
    </section >
  )
}

export default Aboutus
