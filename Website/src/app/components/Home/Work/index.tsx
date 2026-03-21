'use client'
import React, { useRef, useState } from 'react'
import Image from 'next/image'
import { WorkData } from '@/app/data/siteData'
import { motion, useInView, useMotionValue, useSpring, useTransform } from 'framer-motion'

const ROLE_COLORS: Record<string, { from: string; to: string; emoji: string; light: string }> = {
  'UI/UX Developer': { from: '#ff6b6b', to: '#ee0979', emoji: '🎨', light: '#ff6b6b22' },
  'Gameplay Designer': { from: '#7678ed', to: '#3d348b', emoji: '🎮', light: '#7678ed22' },
  'Level Designer': { from: '#f7b801', to: '#f35b03', emoji: '🗺️', light: '#f7b80122' },
  'Cloud Integration Engineer': { from: '#02398a', to: '#7678ed', emoji: '☁️', light: '#02398a22' },
  'Systems Engineer': { from: '#3d348b', to: '#02398a', emoji: '⚙️', light: '#3d348b22' },
  'Backend Developer': { from: '#f35b03', to: '#f7b801', emoji: '🖥️', light: '#f35b0322' },
}

/* ─── Tilt card wrapper ─── */
const TiltCard = ({ children, className }: { children: React.ReactNode; className?: string }) => {
  const ref = useRef<HTMLDivElement>(null)
  const x = useMotionValue(0)
  const y = useMotionValue(0)
  const springX = useSpring(x, { stiffness: 150, damping: 20 })
  const springY = useSpring(y, { stiffness: 150, damping: 20 })
  const rotateY = useTransform(springX, [-0.5, 0.5], [-10, 10])
  const rotateX = useTransform(springY, [-0.5, 0.5], [8, -8])

  const onMouse = (e: React.MouseEvent<HTMLDivElement>) => {
    const rect = ref.current!.getBoundingClientRect()
    x.set((e.clientX - rect.left) / rect.width - 0.5)
    y.set((e.clientY - rect.top) / rect.height - 0.5)
  }
  const onLeave = () => { x.set(0); y.set(0) }

  return (
    <motion.div
      ref={ref}
      onMouseMove={onMouse}
      onMouseLeave={onLeave}
      style={{ rotateY, rotateX, transformStyle: 'preserve-3d', perspective: 1000 }}
      className={className}
    >
      {children}
    </motion.div>
  )
}

/* ─── Individual team card ─── */
const TeamCard = ({ item, index }: { item: any; index: number }) => {
  const accent = ROLE_COLORS[item.profession] ?? { from: '#f35b03', to: '#7678ed', emoji: '✨', light: '#f35b0322' }
  const ref = useRef<HTMLDivElement>(null)
  const inView = useInView(ref, { once: true, margin: '-60px' })
  const [hovered, setHovered] = useState(false)

  return (
    <motion.div
      ref={ref}
      initial={{ opacity: 0, y: 70, scale: 0.92 }}
      animate={inView ? { opacity: 1, y: 0, scale: 1 } : {}}
      transition={{ duration: 0.75, delay: index * 0.12, ease: [0.22, 1, 0.36, 1] }}
    >
      <TiltCard className="h-full">
        <div
          className="relative rounded-3xl overflow-hidden cursor-pointer group max-w-[320px] mx-auto w-full"
          style={{
            height: '400px',
            boxShadow: hovered
              ? `0 30px 70px -10px ${accent.from}55, 0 0 0 1px ${accent.from}33`
              : '0 10px 40px -10px rgba(0,0,0,0.15)',
            transition: 'box-shadow 0.4s ease',
          }}
          onMouseEnter={() => setHovered(true)}
          onMouseLeave={() => setHovered(false)}
        >
          {/* ── Full-bleed scaled-down photo ── */}
          <div className="absolute inset-0">
            <Image
              src={item.imgSrc}
              alt={item.name}
              fill
              className="object-cover object-top transition-transform duration-700 ease-out group-hover:scale-110"
              sizes="(max-width: 768px) 100vw, 33vw"
            />
          </div>

          {/* ── Always-visible gradient scrim at bottom ── */}
          <div
            className="absolute inset-0"
            style={{
              background: `linear-gradient(to top,
                rgba(0,0,0,0.92) 0%,
                rgba(0,0,0,0.55) 35%,
                rgba(0,0,0,0.0) 65%)`,
            }}
          />

          {/* ── Hover colour overlay ── */}
          <div
            className="absolute inset-0 transition-opacity duration-500"
            style={{
              background: `linear-gradient(160deg, ${accent.from}cc 0%, ${accent.to}aa 100%)`,
              opacity: hovered ? 0.55 : 0,
            }}
          />

          {/* ── Index number (top-left) ── */}
          <div
            className="absolute top-4 left-4 z-20 w-9 h-9 rounded-xl flex items-center justify-center text-sm font-black text-white"
            style={{
              background: `linear-gradient(135deg, ${accent.from}, ${accent.to})`,
              boxShadow: `0 4px 14px ${accent.from}77`,
              transform: 'translateZ(30px)',
            }}
          >
            {String(index + 1).padStart(2, '0')}
          </div>

          {/* ── Emoji badge (top-right) ── */}
          <motion.div
            animate={hovered ? { rotate: [0, -15, 15, 0], scale: 1.2 } : { rotate: 0, scale: 1 }}
            transition={{ duration: 0.5 }}
            className="absolute top-4 right-4 z-20 w-10 h-10 rounded-2xl flex items-center justify-center text-xl"
            style={{
              background: 'rgba(255,255,255,0.15)',
              backdropFilter: 'blur(10px)',
              border: '1px solid rgba(255,255,255,0.25)',
              transform: 'translateZ(30px)',
            }}
          >
            {accent.emoji}
          </motion.div>

          {/* ── Bottom info panel ── */}
          <div
            className="absolute bottom-0 left-0 right-0 z-20 p-6"
            style={{ transform: 'translateZ(20px)' }}
          >
            {/* Role pill */}
            <motion.div
              animate={hovered ? { y: 0, opacity: 1 } : { y: 10, opacity: 0.8 }}
              transition={{ duration: 0.35 }}
              className="inline-block px-3 py-1 rounded-full text-xs font-bold mb-3"
              style={{
                background: `linear-gradient(90deg, ${accent.from}, ${accent.to})`,
                color: '#fff',
                boxShadow: `0 2px 10px ${accent.from}66`,
              }}
            >
              {accent.emoji} {item.profession}
            </motion.div>

            {/* Name */}
            <h5
              className="font-black text-white leading-tight mb-1"
              style={{ textShadow: '0 2px 10px rgba(0,0,0,0.5)' }}
            >
              {item.name}
            </h5>

            {/* Hover Social CTA line */}
            <motion.div
              animate={hovered ? { opacity: 1, y: 0, pointerEvents: 'auto' } : { opacity: 0, y: 8, pointerEvents: 'none' }}
              transition={{ duration: 0.3, delay: 0.1 }}
              className="flex items-center gap-3 mt-4 pt-3 border-t relative z-50"
              style={{ borderColor: `${accent.from}44` }}
            >
              <a href={item.github || '#'} target="_blank" rel="noreferrer" className="w-8 h-8 rounded-full bg-white/10 flex items-center justify-center text-white hover:scale-110 transition-transform">
                <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 24 24"><path fillRule="evenodd" d="M12 2C6.477 2 2 6.484 2 12.017c0 4.425 2.865 8.18 6.839 9.504.5.092.682-.217.682-.483 0-.237-.008-.868-.013-1.703-2.782.605-3.369-1.343-3.369-1.343-.454-1.158-1.11-1.466-1.11-1.466-.908-.62.069-.608.069-.608 1.003.07 1.531 1.032 1.531 1.032.892 1.53 2.341 1.088 2.91.832.092-.647.35-1.088.636-1.338-2.22-.253-4.555-1.113-4.555-4.951 0-1.093.39-1.988 1.029-2.688-.103-.253-.446-1.272.098-2.65 0 0 .84-.27 2.75 1.026A9.564 9.564 0 0112 6.844c.85.004 1.705.115 2.504.337 1.909-1.296 2.747-1.027 2.747-1.027.546 1.379.202 2.398.1 2.651.64.7 1.028 1.595 1.028 2.688 0 3.848-2.339 4.695-4.566 4.943.359.309.678.92.678 1.855 0 1.338-.012 2.419-.012 2.747 0 .268.18.58.688.482A10.019 10.019 0 0022 12.017C22 6.484 17.522 2 12 2z" clipRule="evenodd" /></svg>
              </a>
              <a href={item.linkedin || '#'} target="_blank" rel="noreferrer" className="w-8 h-8 rounded-full bg-white/10 flex items-center justify-center text-white hover:scale-110 hover:bg-[#0077b5] transition-all">
                <svg className="w-3.5 h-3.5" fill="currentColor" viewBox="0 0 24 24"><path d="M20.447 20.452h-3.554v-5.569c0-1.328-.027-3.037-1.852-3.037-1.853 0-2.136 1.445-2.136 2.939v5.667H9.351V9h3.414v1.561h.046c.477-.9 1.637-1.85 3.37-1.85 3.601 0 4.267 2.37 4.267 5.455v6.286zM5.337 7.433c-1.144 0-2.063-.926-2.063-2.065 0-1.138.92-2.063 2.063-2.063 1.14 0 2.064.925 2.064 2.063 0 1.139-.925 2.065-2.064 2.065zm1.782 13.019H3.555V9h3.564v11.452zM22.225 0H1.771C.792 0 0 .774 0 1.729v20.542C0 23.227.792 24 1.771 24h20.451C23.2 24 24 23.227 24 22.271V1.729C24 .774 23.2 0 22.222 0h.003z" /></svg>
              </a>
              <a href={item.email ? `mailto:${item.email}` : '#'} className="w-8 h-8 rounded-full bg-white/10 flex items-center justify-center text-white hover:scale-110 hover:bg-[#ea4335] transition-all">
                <svg className="w-4 h-4" fill="none" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" /></svg>
              </a>
              <span className="text-white/40 text-[10px] font-medium tracking-[0.2em] uppercase ml-auto">
                Connect
              </span>
            </motion.div>
          </div>

          {/* ── Shine sweep on hover ── */}
          <motion.div
            animate={hovered ? { x: ['−100%', '200%'] } : { x: '-100%' }}
            transition={{ duration: 0.6, ease: 'easeInOut' }}
            className="absolute inset-0 z-30 pointer-events-none"
            style={{
              background:
                'linear-gradient(105deg, transparent 40%, rgba(255,255,255,0.18) 50%, transparent 60%)',
            }}
          />
        </div>
      </TiltCard>
    </motion.div>
  )
}

/* ─── Section ─── */
const Work = () => {
  const headerRef = useRef<HTMLDivElement>(null)
  const headerInView = useInView(headerRef, { once: true })

  return (
    <section
      id="Team"
      className="relative py-24 overflow-hidden"
      style={{ background: 'linear-gradient(180deg, #f8f8ff 0%, #fff5ee 100%)' }}
    >
      {/* Decorative blobs */}
      <div
        className="absolute -top-32 left-1/2 -translate-x-1/2 w-[600px] h-[600px] rounded-full opacity-[0.07] animate-morph pointer-events-none"
        style={{ background: 'radial-gradient(circle, #7678ed, #f35b03)', filter: 'blur(80px)' }}
      />
      <div
        className="absolute bottom-0 left-0 w-72 h-72 rounded-full opacity-[0.06] animate-morph pointer-events-none"
        style={{ background: 'radial-gradient(circle, #f35b03, transparent)', filter: 'blur(60px)', animationDelay: '4s' }}
      />
      <div
        className="absolute bottom-0 right-0 w-72 h-72 rounded-full opacity-[0.06] animate-morph pointer-events-none"
        style={{ background: 'radial-gradient(circle, #7678ed, transparent)', filter: 'blur(60px)', animationDelay: '2s' }}
      />

      <div className="container mx-auto max-w-7xl px-4 relative z-10">
        {/* ── Section header ── */}
        <motion.div
          ref={headerRef}
          initial={{ opacity: 0, y: 40 }}
          animate={headerInView ? { opacity: 1, y: 0 } : {}}
          transition={{ duration: 0.8 }}
          className="text-center mb-16"
        >
          <div className="inline-flex items-center gap-2 py-2 px-5 rounded-full bg-darkmode/10 border border-darkmode/20 mb-5">
            <span className="w-2 h-2 rounded-full bg-darkmode animate-pulse" />
            <p className="text-darkmode text-sm font-bold tracking-widest uppercase">
              Meet the Makers
            </p>
          </div>

          <h2 className="text-black">
            Our{' '}
            <span className="gradient-text">Dream Team.</span>
          </h2>

          <p className="text-black/50 text-lg mt-4 max-w-xl mx-auto leading-relaxed">
            Passionate undergraduates turning ideas into immersive educational experiences.{' '}
            <span className="text-primary font-semibold">Hover to reveal ✨</span>
          </p>

          {/* Decorative line */}
          <div className="flex items-center justify-center gap-4 mt-8">
            <div className="h-px w-24" style={{ background: 'linear-gradient(90deg, transparent, #f35b03)' }} />
            <div className="w-2 h-2 rounded-full bg-primary animate-pulse" />
            <div className="h-px w-24" style={{ background: 'linear-gradient(90deg, #7678ed, transparent)' }} />
          </div>
        </motion.div>

        {/* ── Cards grid ──
              On large screens: 3 columns, middle card is taller (featured) via CSS grid rows */}
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
          {WorkData.map((item, i) => (
            <TeamCard key={i} item={item} index={i} />
          ))}
        </div>

        {/* ── Bottom team tag ── */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={headerInView ? { opacity: 1, y: 0 } : {}}
          transition={{ duration: 0.7, delay: 0.9 }}
          className="text-center mt-14"
        >
          <div
            className="inline-flex items-center gap-3 rounded-full px-6 py-3 text-sm font-semibold"
            style={{
              background: 'linear-gradient(135deg, rgba(243,91,3,0.08), rgba(118,120,237,0.08))',
              border: '1px solid rgba(243,91,3,0.15)',
              color: '#3d348b',
            }}
          >
            <span className="text-base">🎓</span>
            IIT · University of Westminster · Sri Lanka
            <span className="text-base">🇱🇰</span>
          </div>
        </motion.div>
      </div>
    </section>
  )
}

export default Work
