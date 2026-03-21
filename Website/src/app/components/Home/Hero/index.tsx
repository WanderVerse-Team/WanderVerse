'use client'
import Link from 'next/link'
import Image from 'next/image'
import { motion, useMotionValue, useSpring, useTransform, useScroll } from 'framer-motion'
import { useEffect, useRef, useState } from 'react'

// Fixed deterministic particle data (no Math.random at module level → no hydration mismatch)
const PARTICLES = [
  { id: 0, size: 8, left: '5%', delay: 0, duration: 14, color: '#f35b03' },
  { id: 1, size: 5, left: '12%', delay: 2.5, duration: 18, color: '#7678ed' },
  { id: 2, size: 10, left: '20%', delay: 1, duration: 15, color: '#f7b801' },
  { id: 3, size: 6, left: '28%', delay: 3.5, duration: 20, color: '#f35b03' },
  { id: 4, size: 9, left: '35%', delay: 0.5, duration: 13, color: '#7678ed' },
  { id: 5, size: 4, left: '42%', delay: 5, duration: 17, color: '#f7b801' },
  { id: 6, size: 11, left: '50%', delay: 1.5, duration: 22, color: '#f35b03' },
  { id: 7, size: 7, left: '57%', delay: 4, duration: 16, color: '#7678ed' },
  { id: 8, size: 5, left: '63%', delay: 2, duration: 19, color: '#f7b801' },
  { id: 9, size: 8, left: '70%', delay: 6, duration: 14, color: '#f35b03' },
  { id: 10, size: 12, left: '75%', delay: 0.8, duration: 21, color: '#7678ed' },
  { id: 11, size: 4, left: '80%', delay: 3, duration: 18, color: '#f7b801' },
  { id: 12, size: 9, left: '85%', delay: 1.2, duration: 15, color: '#f35b03' },
  { id: 13, size: 6, left: '88%', delay: 4.5, duration: 23, color: '#7678ed' },
  { id: 14, size: 7, left: '92%', delay: 2.8, duration: 16, color: '#f7b801' },
  { id: 15, size: 5, left: '18%', delay: 7, duration: 20, color: '#f35b03' },
  { id: 16, size: 10, left: '45%', delay: 3.2, duration: 12, color: '#7678ed' },
  { id: 17, size: 8, left: '96%', delay: 5.5, duration: 17, color: '#f7b801' },
]

const TYPED_WORDS = ['Fun.', 'Play.', 'Magic.', 'Joy.', 'Growth.']

const TypingText = () => {
  const [index, setIndex] = useState(0)
  const [displayed, setDisplayed] = useState('')
  const [deleting, setDeleting] = useState(false)

  useEffect(() => {
    const word = TYPED_WORDS[index]
    let timeout: ReturnType<typeof setTimeout> | undefined = undefined

    if (!deleting && displayed.length < word.length) {
      timeout = setTimeout(() => setDisplayed(word.slice(0, displayed.length + 1)), 80)
    } else if (!deleting && displayed.length === word.length) {
      timeout = setTimeout(() => setDeleting(true), 1800)
    } else if (deleting && displayed.length > 0) {
      timeout = setTimeout(() => setDisplayed(displayed.slice(0, -1)), 45)
    } else if (deleting && displayed.length === 0) {
      setDeleting(false)
      setIndex((prev) => (prev + 1) % TYPED_WORDS.length)
    }

    return () => clearTimeout(timeout)
  }, [displayed, deleting, index])

  return (
    <span className="gradient-text">
      {displayed}
      <span className="animate-pulse text-primary">|</span>
    </span>
  )
}

const FloatingOrb = ({
  size,
  color,
  x,
  y,
  delay,
}: {
  size: number
  color: string
  x: string
  y: string
  delay: number
}) => (
  <div
    className="absolute pointer-events-none animate-morph"
    style={{
      width: size,
      height: size,
      left: x,
      top: y,
      background: `radial-gradient(circle, ${color}60, ${color}10)`,
      filter: 'blur(30px)',
      animationDelay: `${delay}s`,
    }}
  />
)

const Hero = () => {
  const containerRef = useRef<HTMLDivElement>(null)
  const mouseX = useMotionValue(0)
  const mouseY = useMotionValue(0)
  const springX = useSpring(mouseX, { stiffness: 40, damping: 30 })
  const springY = useSpring(mouseY, { stiffness: 40, damping: 30 })
  const imgX = useTransform(springX, [-300, 300], [-12, 12])
  const imgY = useTransform(springY, [-300, 300], [-6, 6])

  useEffect(() => {
    const handleMove = (e: MouseEvent) => {
      const rect = containerRef.current?.getBoundingClientRect()
      if (!rect) return
      mouseX.set(e.clientX - rect.left - rect.width / 2)
      mouseY.set(e.clientY - rect.top - rect.height / 2)
    }
    window.addEventListener('mousemove', handleMove)
    return () => window.removeEventListener('mousemove', handleMove)
  }, [mouseX, mouseY])
  // Scroll-exit parallax
  const { scrollY } = useScroll()
  const contentOpacity = useTransform(scrollY, [0, 450], [1, 0])
  const contentY = useTransform(scrollY, [0, 450], [0, -70])
  const contentScale = useTransform(scrollY, [0, 450], [1, 0.94])
  const heroBgScale = useTransform(scrollY, [0, 600], [1, 1.08])
  // Dark overlay fades IN as content fades out → hides bare hero background
  const heroOverlayOpacity = useTransform(scrollY, [200, 680], [0, 1])


  return (
    <section
      ref={containerRef}
      className="relative overflow-hidden z-1 h-[100svh] flex flex-col"
      style={{
        background:
          'linear-gradient(135deg, #faf5f0 0%, #f0f0ff 40%, #fff5ee 70%, #f0f5ff 100%)',
      }}
    >
      {/* Background — scales up slightly on scroll for parallax depth */}
      <motion.div
        className="absolute inset-0"
        style={{ scale: heroBgScale }}
      >
        <FloatingOrb size={500} color="#f35b03" x="-8%" y="-10%" delay={0} />
        <FloatingOrb size={400} color="#7678ed" x="60%" y="50%" delay={3} />
        <FloatingOrb size={300} color="#f7b801" x="30%" y="70%" delay={1.5} />
      </motion.div>

      {/* Dark overlay — covers bare background as content fades, bridges to dark panels */}
      <motion.div
        className="absolute inset-0 pointer-events-none"
        style={{ opacity: heroOverlayOpacity, background: '#0d0a04', zIndex: 5 }}
      />

      {/* Floating particles (not affected by scroll scale) */}
      {PARTICLES.map((p) => (
        <div
          key={p.id}
          className="absolute rounded-full pointer-events-none"
          style={{
            width: p.size,
            height: p.size,
            left: p.left,
            bottom: '-20px',
            background: p.color,
            opacity: 0.6,
            animation: `float-up ${p.duration}s ease-in-out ${p.delay}s infinite`,
          }}
        />
      ))}

      {/* Grid pattern overlay */}
      <div
        className="absolute inset-0 opacity-[0.025]"
        style={{
          backgroundImage:
            'linear-gradient(#000 1px, transparent 1px), linear-gradient(90deg, #000 1px, transparent 1px)',
          backgroundSize: '60px 60px',
        }}
      />

      {/* Spacer to guarantee content never overlaps the fixed header under any screen height */}
      <div className="h-4 md:h-12 shrink-0 w-full pointer-events-none relative z-0" />

      {/* All foreground content fades + moves up as user scrolls */}
      <motion.div
        style={{ opacity: contentOpacity, y: contentY, scale: contentScale }}
        className="container mx-auto pb-12 max-w-7xl px-4 relative z-10 w-full my-auto"
      >
        <div className="grid grid-cols-12 justify-center items-center gap-8">
          {/* LEFT COLUMN */}
          <div className="col-span-12 xl:col-span-6 lg:col-span-6 md:col-span-12">
            {/* Badge */}
            <motion.div
              initial={{ opacity: 0, y: 40 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.7, delay: 0 }}
              className="mb-6"
            >
              <div className="inline-flex items-center gap-2 py-2 px-5 rounded-full border border-primary/30 bg-primary/10 backdrop-blur-sm">
                <span className="w-2 h-2 rounded-full bg-primary animate-pulse" />
                <p className="text-primary text-sm font-bold tracking-widest uppercase">
                  Educational App
                </p>
              </div>
            </motion.div>

            {/* Headline */}
            <motion.div
              initial={{ opacity: 0, y: 40 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.7, delay: 0.18 }}
            >
              <h1 className="text-black leading-tight mb-2">
                Where learning
                <br />
                meets{' '}
                <TypingText />
              </h1>
            </motion.div>

            {/* Subtext */}
            <motion.p
              initial={{ opacity: 0, y: 40 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.7, delay: 0.36 }}
              className="text-xl text-black/60 mt-6 mb-10 max-w-md leading-relaxed"
            >
              Dive into Willow's world — where every lesson is a game, every level
              makes you smarter, and education feels like play.
            </motion.p>

            {/* CTA Buttons */}
            <motion.div
              initial={{ opacity: 0, y: 40 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.7, delay: 0.54 }}
              className="flex flex-wrap gap-4"
            >
              <Link href="#">
                <motion.button
                  whileHover={{ scale: 1.04 }}
                  whileTap={{ scale: 0.97 }}
                  className="btn-ripple relative bg-primary text-white text-lg font-bold py-4 px-10 rounded-full shadow-lg hover:shadow-glow transition-shadow duration-300"
                >
                  <span className="relative z-10">🚀 DOWNLOAD NOW</span>
                </motion.button>
              </Link>
              <motion.button
                whileHover={{ scale: 1.04 }}
                whileTap={{ scale: 0.97 }}
                onClick={() => window.scrollTo({ top: window.innerHeight, behavior: 'smooth' })}
                className="text-lg font-semibold py-4 px-10 rounded-full border-2 border-darkmode text-darkmode hover:bg-darkmode hover:text-white transition-colors duration-300"
              >
                Learn More ↓
              </motion.button>
            </motion.div>

            {/* Stats row */}
            <motion.div
              initial={{ opacity: 0, y: 40 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.7, delay: 0.72 }}
              className="flex gap-8 mt-12 pt-8 border-t border-black/10"
            >
              {[
                { value: '3rd', label: 'Grade Focus' },
                { value: '100%', label: 'Gamified' },
                { value: 'Free', label: 'To Download' },
              ].map((stat) => (
                <div key={stat.label}>
                  <p className="text-3xl font-black gradient-text">{stat.value}</p>
                  <p className="text-sm text-black/50 font-medium mt-1">{stat.label}</p>
                </div>
              ))}
            </motion.div>
          </div>


          {/* RIGHT COLUMN — hero image with parallax */}
          <div className="xl:col-span-6 lg:col-span-6 lg:block hidden">
            <motion.div
              style={{ x: imgX, y: imgY }}
              initial={{ opacity: 0, scale: 0.9 }}
              animate={{ opacity: 1, scale: 1 }}
              transition={{ duration: 1, ease: [0.22, 1, 0.36, 1] as [number, number, number, number], delay: 0.3 }}
              className="relative"
            >
              {/* Decorative ring */}
              <motion.div
                animate={{ rotate: 360 }}
                transition={{ duration: 30, repeat: Infinity, ease: 'linear' }}
                className="absolute inset-0 m-auto"
                style={{
                  width: '85%',
                  height: '85%',
                  border: '2px dashed rgba(243,91,3,0.2)',
                  borderRadius: '50%',
                  top: '7.5%',
                  left: '7.5%',
                }}
              />

              {/* Floating badge 1 */}
              <motion.div
                animate={{ y: [-8, 8, -8] }}
                transition={{ duration: 4, repeat: Infinity, ease: 'easeInOut' }}
                className="absolute -left-8 top-16 glass-card rounded-2xl p-4 shadow-xl z-20"
              >
                <div className="flex items-center gap-3">
                  <div className="w-10 h-10 rounded-xl bg-primary/20 flex items-center justify-center text-2xl">⭐</div>
                  <div>
                    <p className="text-xs font-bold text-black">Gamified Learning</p>
                    <p className="text-xs text-black/50">Math Made Fun!</p>
                  </div>
                </div>
              </motion.div>

              {/* Floating badge 2 */}
              <motion.div
                animate={{ y: [8, -8, 8] }}
                transition={{ duration: 5, repeat: Infinity, ease: 'easeInOut', delay: 1 }}
                className="absolute -right-4 bottom-24 glass-card rounded-2xl p-4 shadow-xl z-20"
              >
                <div className="flex items-center gap-3">
                  <div className="w-10 h-10 rounded-xl bg-purple/20 flex items-center justify-center text-2xl">🎮</div>
                  <div>
                    <p className="text-xs font-bold text-black">Play & Learn</p>
                    <p className="text-xs text-black/50">Level Up!</p>
                  </div>
                </div>
              </motion.div>

              <Image
                src="/images/hero/banner-image.png"
                alt="WanderVerse Hero"
                width={700}
                height={700}
                className="w-full relative z-10 drop-shadow-2xl"
                priority
              />
            </motion.div>
          </div>
        </div>
      </motion.div>

      {/* Bottom wave */}
      <div className="absolute bottom-0 left-0 w-full overflow-hidden leading-none">
        <svg
          viewBox="0 0 1440 80"
          xmlns="http://www.w3.org/2000/svg"
          className="w-full h-auto"
          preserveAspectRatio="none"
        >
          <path
            d="M0,40 C360,80 1080,0 1440,40 L1440,80 L0,80 Z"
            fill="white"
            opacity="0.8"
          />
        </svg>
      </div>
    </section>
  )
}

export default Hero
