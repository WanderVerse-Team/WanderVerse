'use client'
import React, { useRef, useState } from 'react'
import { motion, AnimatePresence, useInView } from 'framer-motion'

const FAQS = [
  {
    q: 'Is WanderVerse available for all grades?',
    a: 'Currently, no. The initial phase of the app only covers Mathematics for the third grade. We plan to expand to more grades in future updates!',
    emoji: '🎓',
  },
  {
    q: 'Is the game available in English medium?',
    a: 'No. Third grade Mathematics is only taught in Sinhala and Tamil mediums in Sri Lanka. So the initial phase of the project will only focus on Sinhala medium.',
    emoji: '🌐',
  },
  {
    q: 'Where are you located?',
    a: 'The WanderVerse team is based in Sri Lanka. We are a team of undergraduate students from the Informatics Institute of Technology (IIT), affiliated with the University of Westminster.',
    emoji: '📍',
  },
  {
    q: 'Is WanderVerse free to download?',
    a: 'Yes! WanderVerse is completely free to download and play. Our mission is to make quality education accessible to all third-grade students in Sri Lanka.',
    emoji: '🎁',
  },
  {
    q: 'What devices does WanderVerse support?',
    a: 'WanderVerse is a Unity-based mobile game available for both Android and iOS devices. Any modern smartphone or tablet can run the app smoothly.',
    emoji: '📱',
  },
]

const FAQItem = ({
  item,
  index,
  isOpen,
  onToggle,
}: {
  item: (typeof FAQS)[0]
  index: number
  isOpen: boolean
  onToggle: () => void
}) => (
  <motion.div
    initial={{ opacity: 0, y: 20 }}
    animate={{ opacity: 1, y: 0 }}
    transition={{ duration: 0.5, delay: index * 0.08 }}
    className={`rounded-2xl border-2 overflow-hidden transition-all duration-300 ${isOpen
        ? 'border-primary/40 shadow-lg bg-white'
        : 'border-transparent bg-white/80 hover:border-primary/20 hover:shadow-md'
      }`}
  >
    <button
      onClick={onToggle}
      className="flex w-full items-center gap-4 px-6 py-5 text-left hover:cursor-pointer"
      aria-expanded={isOpen}
    >
      {/* Emoji */}
      <div
        className={`w-10 h-10 rounded-xl flex items-center justify-center text-xl flex-shrink-0 transition-all duration-300 ${isOpen ? 'bg-primary text-white scale-110' : 'bg-primary/10'
          }`}
      >
        {item.emoji}
      </div>

      <span className="flex-1 text-lg font-bold text-black">{item.q}</span>

      {/* Arrow */}
      <motion.span
        animate={{ rotate: isOpen ? 180 : 0 }}
        transition={{ duration: 0.35 }}
        className={`flex-shrink-0 w-8 h-8 rounded-full flex items-center justify-center text-sm font-bold transition-colors duration-300 ${isOpen ? 'bg-primary text-white' : 'bg-black/5 text-black/40'
          }`}
      >
        ↓
      </motion.span>
    </button>

    <AnimatePresence initial={false}>
      {isOpen && (
        <motion.div
          key="content"
          initial={{ height: 0, opacity: 0 }}
          animate={{ height: 'auto', opacity: 1 }}
          exit={{ height: 0, opacity: 0 }}
          transition={{ duration: 0.35, ease: [0.22, 1, 0.36, 1] }}
          className="overflow-hidden"
        >
          <div className="px-6 pb-6 pt-2 ml-14">
            <div className="h-px w-full bg-primary/10 mb-4" />
            <p className="text-black/70 text-base leading-relaxed">{item.a}</p>
          </div>
        </motion.div>
      )}
    </AnimatePresence>
  </motion.div>
)

const FAQ = () => {
  const [openIndex, setOpenIndex] = useState<number | null>(0)
  const ref = useRef<HTMLDivElement>(null)
  const inView = useInView(ref, { once: true, margin: '-60px' })

  return (
    <section id="FAQ" className="relative overflow-hidden py-20">
      {/* Background */}
      <div
        className="absolute inset-0"
        style={{
          background:
            'linear-gradient(160deg, #f35b03 0%, #3d348b 50%, #02398a 100%)',
        }}
      />
      {/* Animated gradient overlay */}
      <div
        className="absolute inset-0 animate-gradient opacity-25"
        style={{
          background: 'linear-gradient(270deg, #7678ed, #f35b03, #f7b801, #7678ed)',
          backgroundSize: '400% 400%',
        }}
      />
      {/* Grid pattern */}
      <div
        className="absolute inset-0 opacity-10"
        style={{
          backgroundImage:
            'linear-gradient(rgba(255,255,255,0.15) 1px, transparent 1px), linear-gradient(90deg, rgba(255,255,255,0.15) 1px, transparent 1px)',
          backgroundSize: '50px 50px',
        }}
      />

      {/* Orbs */}
      <div
        className="absolute -left-32 top-1/4 w-96 h-96 animate-morph opacity-20"
        style={{ background: 'rgba(255,255,255,0.1)', filter: 'blur(60px)' }}
      />
      <div
        className="absolute -right-32 bottom-1/4 w-96 h-96 animate-morph opacity-20"
        style={{
          background: 'rgba(247,184,1,0.3)',
          filter: 'blur(60px)',
          animationDelay: '4s',
        }}
      />

      <div className="container mx-auto max-w-7xl px-4 relative z-10">
        {/* Header */}
        <motion.div
          ref={ref}
          initial={{ opacity: 0, y: 30 }}
          animate={inView ? { opacity: 1, y: 0 } : {}}
          transition={{ duration: 0.7 }}
          className="text-center mb-12"
        >
          <div className="inline-flex items-center gap-2 py-2 px-5 rounded-full bg-white/15 border border-white/20 mb-4 backdrop-blur-sm">
            <span className="w-2 h-2 rounded-full bg-yellow-400 animate-pulse" />
            <p className="text-white/80 text-sm font-bold tracking-widest uppercase">
              FAQ
            </p>
          </div>
          <h2 className="text-white mb-4">
            Frequently Asked{' '}
            <span
              style={{
                background: 'linear-gradient(90deg, #f7b801, #fff)',
                WebkitBackgroundClip: 'text',
                WebkitTextFillColor: 'transparent',
                backgroundClip: 'text',
              }}
            >
              Questions.
            </span>
          </h2>
          <p className="text-white/60 text-lg max-w-xl mx-auto">
            Got questions? We've got answers. Can't find what you need? Reach out to
            the team anytime.
          </p>
        </motion.div>

        {/* FAQ list */}
        <motion.div
          initial={{ opacity: 0 }}
          animate={inView ? { opacity: 1 } : {}}
          transition={{ duration: 0.6, delay: 0.3 }}
          className="max-w-4xl mx-auto space-y-3"
        >
          {FAQS.map((item, i) => (
            <FAQItem
              key={i}
              item={item}
              index={i}
              isOpen={openIndex === i}
              onToggle={() => setOpenIndex(openIndex === i ? null : i)}
            />
          ))}
        </motion.div>

        {/* Bottom CTA */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={inView ? { opacity: 1, y: 0 } : {}}
          transition={{ duration: 0.6, delay: 0.8 }}
          className="text-center mt-12"
        >
          <p className="text-white/60 text-lg">
            Still have questions?{' '}
            <a
              href="mailto:team@wanderverse.com"
              className="text-yellow-400 font-bold hover:underline"
            >
              Contact our team →
            </a>
          </p>
        </motion.div>
      </div>
    </section>
  )
}

export default FAQ
