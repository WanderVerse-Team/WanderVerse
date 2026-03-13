'use client'
import { useRef, useEffect } from 'react'
import { motion, useScroll, useTransform, useSpring, MotionValue } from 'framer-motion'

// ─── Panel definitions ────────────────────────────────────────────────────────
const PANELS = [
    {
        id: 'game',
        label: '01 / THE GAME',
        headline: ['Where', 'Learning', 'Meets Play.'],
        sub: 'WanderVerse turns the Grade 3 Sri Lankan Mathematics syllabus into an immersive, gamified adventure with Willow the Otter.',
        accent: '#f35b03',
        bg: '#0d0a04',
        emoji: '🎮',
        stats: [{ v: '10+', l: 'Levels' }, { v: 'Grade 3', l: 'Focus' }, { v: '∞', l: 'Fun' }],
    },
    {
        id: 'how',
        label: '02 / HOW IT WORKS',
        headline: ['Gamification', 'of Every', 'Lesson.'],
        sub: 'Each curriculum concept is broken into levels, challenges and rewards — so children learn without even realising it.',
        accent: '#7678ed',
        bg: '#06060f',
        emoji: '🧠',
        stats: [{ v: '100%', l: 'Gamified' }, { v: 'Free', l: 'Download' }, { v: '🇱🇰', l: 'Sri Lanka' }],
    },
    {
        id: 'mission',
        label: '03 / OUR MISSION',
        headline: ['Education,', 'Reimagined', 'for All.'],
        sub: 'Built by IIT students for Sri Lankan Grade 3 classrooms — bridging school education, modern trends, and technology.',
        accent: '#f7b801',
        bg: '#0a0800',
        emoji: '🚀',
        stats: [{ v: 'IIT', l: 'University' }, { v: '6', l: 'Makers' }, { v: '2024', l: 'Founded' }],
    },
]

// ─── Per‑panel component — all hooks at top level, no conditionals ─────────────
function Panel({
    panel,
    pi,
    smooth,
    total,
}: {
    panel: (typeof PANELS)[0]
    pi: number
    smooth: MotionValue<number>
    total: number
}) {
    const s = pi / total
    const e = (pi + 1) / total

    // ── Panel 0 centres at progress 0, Panel 1 at 0.425, Panel 2 at 0.85 ──
    let points: number[], o: number[], yPos: number[], sc: number[]
    if (pi === 0) {
        points = [0, 0.05, 0.2]
        o = [1, 1, 0]
        yPos = [0, 0, -25]
        sc = [1, 1, 0.93]
    } else if (pi === 1) {
        points = [0.2, 0.35, 0.5, 0.65]
        o = [0, 1, 1, 0]
        yPos = [40, 0, 0, -25]
        sc = [1.15, 1, 1, 0.93]
    } else {
        points = [0.65, 0.8, 0.9]
        o = [0, 1, 1]
        yPos = [40, 0, 0]
        sc = [1.15, 1, 1]
    }

    const scale = useTransform(smooth, points, sc)
    const opacity = useTransform(smooth, points, o)
    const y = useTransform(smooth, points, yPos)

    // Sub-elements share the exact same animation window
    const subOpacity = opacity
    const subY = useTransform(smooth, points, yPos)
    const statOp = opacity

    const bgShift = useTransform(smooth, [0, 1], ['5%', '-5%'])

    // Progress bar fills while the panel is central
    let barPoints: number[], barVals: string[]
    if (pi === 0) {
        barPoints = [0, 0.2]; barVals = ['100%', '0%']
    } else if (pi === 1) {
        barPoints = [0.2, 0.35, 0.5, 0.65]; barVals = ['0%', '100%', '100%', '0%']
    } else {
        barPoints = [0.65, 0.8]; barVals = ['0%', '100%']
    }
    const barW = useTransform(smooth, barPoints, barVals)
    const hintOp = useTransform(smooth, [0, 0.15], [1, 0])

    const sideH = useTransform(smooth, [s, e], ['0%', '100%'])

    return (
        <>
            {/* ── Panel — positioned absolute in the 100vw/100vh sticky container ── */}
            {/* We translate each panel: panel 0 starts at 0, panel 1 at 100vw, etc.  */}
            {/* The rail itself is moved by x; panels stay fixed relative to the rail. */}
            <div
                className="absolute top-0 left-0 w-screen h-screen flex items-center overflow-hidden"
                style={{
                    background: panel.bg,
                    // Panels are laid out side by side via marginLeft on a flex‑child approach,
                    // but since we're absolute, we offset them manually.
                    // This is handled by the parent flex rail instead — see below.
                }}
            >
                {/* Parallax bg layer */}
                <motion.div className="absolute inset-0 pointer-events-none" style={{ x: bgShift, scale: 1.06 }}>
                    <div
                        className="absolute inset-0 opacity-[0.035]"
                        style={{
                            backgroundImage:
                                'linear-gradient(rgba(255,255,255,0.2) 1px, transparent 1px), linear-gradient(90deg, rgba(255,255,255,0.2) 1px, transparent 1px)',
                            backgroundSize: '70px 70px',
                        }}
                    />
                    <div
                        className="absolute w-[600px] h-[600px] rounded-full"
                        style={{
                            background: `radial-gradient(circle, ${panel.accent}35, transparent 65%)`,
                            filter: 'blur(90px)',
                            top: '5%',
                            left: '-5%',
                        }}
                    />
                    <div
                        className="absolute w-[350px] h-[350px] rounded-full"
                        style={{
                            background: `radial-gradient(circle, ${panel.accent}20, transparent 65%)`,
                            filter: 'blur(60px)',
                            bottom: '0%',
                            right: '8%',
                        }}
                    />
                </motion.div>

                {/* Giant watermark */}
                <div
                    className="absolute inset-0 flex items-center justify-center pointer-events-none overflow-hidden"
                >
                    <span
                        className="font-black whitespace-nowrap pointer-events-auto"
                        style={{ fontSize: 'clamp(80px, 14vw, 220px)', color: panel.accent, opacity: 0.025 }}
                        data-cursor="WANDERVERSE"
                    >
                        WANDERVERSE
                    </span>
                </div>

                {/* Label top‑left */}
                <div className="absolute top-8 md:top-10 left-6 md:left-8 lg:left-16 z-20">
                    <motion.p style={{ color: panel.accent, opacity }} className="text-[10px] md:text-xs font-black tracking-[0.35em] uppercase">
                        {panel.label}
                    </motion.p>
                </div>

                {/* Dots top‑right */}
                <div className="absolute top-8 md:top-10 right-6 md:right-8 lg:right-20 z-20 hidden sm:flex gap-2 items-center">
                    {PANELS.map((_, di) => (
                        <div
                            key={di}
                            className="rounded-full transition-all duration-500"
                            style={{
                                width: di === pi ? '22px' : '7px',
                                height: '7px',
                                background: di === pi ? panel.accent : 'rgba(255,255,255,0.18)',
                            }}
                        />
                    ))}
                </div>

                {/* Content grid */}
                <div className="relative z-10 w-full max-w-7xl mx-auto px-6 sm:px-8 lg:px-20 grid grid-cols-1 lg:grid-cols-2 gap-8 lg:gap-16 items-center pt-16 sm:pt-0">
                    {/* LEFT — headline */}
                    <div className="flex flex-col items-center lg:items-start text-center lg:text-left">
                        <motion.div style={{ scale, opacity, y }} className="origin-center lg:origin-left">
                            <h2
                                className="font-black leading-[0.85] lg:leading-[0.88]"
                                style={{ fontSize: 'clamp(36px, 6.5vw, 100px)', color: 'white' }}
                            >
                                {panel.headline.map((line, li) => (
                                    <span
                                        key={li}
                                        className="block"
                                        style={{ color: li === 0 ? panel.accent : 'white' }}
                                    >
                                        {line}
                                    </span>
                                ))}
                            </h2>
                        </motion.div>

                        <motion.p
                            style={{ opacity: subOpacity, y: subY }}
                            className="text-white/50 text-base md:text-lg leading-relaxed mt-5 md:mt-7 max-w-md mx-auto lg:mx-0"
                        >
                            {panel.sub}
                        </motion.p>

                        <motion.div
                            className="flex gap-6 md:gap-8 mt-8 md:mt-10 pt-6 md:pt-8 border-t justify-center lg:justify-start"
                            style={{ borderColor: `${panel.accent}25`, opacity: statOp }}
                        >
                            {panel.stats.map((s, si) => (
                                <div key={si} className="flex flex-col items-center lg:items-start">
                                    <span className="text-xl md:text-2xl font-black" style={{ color: panel.accent }}>{s.v}</span>
                                    <span className="text-white/35 text-[10px] md:text-xs font-bold tracking-widest uppercase mt-0.5">{s.l}</span>
                                </div>
                            ))}
                        </motion.div>
                    </div>

                    {/* RIGHT — orb */}
                    <motion.div style={{ opacity: subOpacity }} className="flex items-center justify-center scale-50 sm:scale-75 lg:scale-100 transform -mt-12 sm:-mt-8 lg:mt-0">
                        <div className="relative flex items-center justify-center w-72 h-72">
                            <motion.div
                                animate={{ rotate: 360 }}
                                transition={{ duration: 22, repeat: Infinity, ease: 'linear' }}
                                className="absolute inset-0 rounded-full"
                                style={{ border: `2px dashed ${panel.accent}35` }}
                            />
                            <motion.div
                                animate={{ rotate: -360 }}
                                transition={{ duration: 35, repeat: Infinity, ease: 'linear' }}
                                className="absolute rounded-full"
                                style={{ inset: '24px', border: `1px dashed ${panel.accent}20` }}
                            />
                            <div
                                className="relative w-44 h-44 rounded-full flex items-center justify-center interactable-emoji cursor-pointer"
                                style={{
                                    background: `radial-gradient(circle, ${panel.accent}18, transparent 70%)`,
                                    border: `1px solid ${panel.accent}28`,
                                    boxShadow: `0 0 60px ${panel.accent}28`,
                                }}
                                data-cursor="EXPLORE"
                            >
                                <motion.span
                                    animate={{ scale: [1, 1.1, 1], rotate: [0, 4, -4, 0] }}
                                    transition={{ duration: 4, repeat: Infinity, ease: 'easeInOut' }}
                                    style={{ fontSize: '5rem' }}
                                >
                                    {panel.emoji}
                                </motion.span>
                            </div>
                        </div>
                    </motion.div>
                </div>

                {/* Scroll hint — first panel only */}
                {pi === 0 && (
                    <motion.div
                        style={{ opacity: hintOp }}
                        className="absolute bottom-8 left-1/2 -translate-x-1/2 z-20 flex flex-col items-center gap-2"
                    >
                        <span className="text-white/25 text-xs tracking-[0.25em] uppercase font-semibold">Scroll to explore</span>
                        <motion.div
                            animate={{ y: [0, 7, 0] }}
                            transition={{ duration: 1.5, repeat: Infinity, ease: 'easeInOut' }}
                            className="w-5 h-8 rounded-full flex items-start justify-center pt-1.5"
                            style={{ border: `1px solid ${panel.accent}40` }}
                        >
                            <div className="w-1 h-2 rounded-full" style={{ background: panel.accent }} />
                        </motion.div>
                    </motion.div>
                )}

                {/* Bottom fill bar */}
                <motion.div
                    className="absolute bottom-0 left-0 h-[2px]"
                    style={{ width: barW, background: `linear-gradient(90deg, ${panel.accent}, ${panel.accent}30)` }}
                />
            </div>
        </>
    )
}

// ─────────────────────────────────────────────────────────────────────────────
export default function HorizontalScroll() {
    const containerRef = useRef<HTMLDivElement>(null)

    const { scrollYProgress } = useScroll({
        target: containerRef,
        offset: ['start start', 'end end'],
    })
    const smooth = useSpring(scrollYProgress, { stiffness: 40, damping: 22, mass: 0.8 })

    // ── Scroll-snap: after user stops scrolling, settle to nearest panel centre ──
    useEffect(() => {
        let timer: ReturnType<typeof setTimeout>

        const snap = () => {
            const el = containerRef.current
            if (!el) return
            const scrollY = window.scrollY
            const sectionScrollable = el.offsetHeight - window.innerHeight
            const secTop = el.offsetTop

            // Only snap if the user's viewport is fully inside this section bounds.
            if (scrollY <= secTop || scrollY >= secTop + sectionScrollable) return

            // The panels are at progress 0, 0.425, and 0.85
            const snapPoints = PANELS.map((_, pi) =>
                secTop + (pi * 0.425) * sectionScrollable
            )

            const target = snapPoints.reduce((a, b) =>
                Math.abs(b - scrollY) < Math.abs(a - scrollY) ? b : a
            )

            window.scrollTo({ top: target, behavior: 'smooth' })
        }

        const onScroll = () => {
            clearTimeout(timer)
            timer = setTimeout(snap, 220)          // wait 220 ms after last scroll event
        }

        window.addEventListener('scroll', onScroll, { passive: true })
        return () => {
            window.removeEventListener('scroll', onScroll)
            clearTimeout(timer)
        }
    }, [])

    // Rail moves left: 0 → –(n-1)×100vw, but finishes early at 0.85
    const railX = useTransform(smooth, [0, 0.85, 1], ['0vw', `-${(PANELS.length - 1) * 100}vw`, `-${(PANELS.length - 1) * 100}vw`])

    // Smooth transition back to light theme at the very end of the scroll (0.88 -> 1.0)
    const exitOverlayOpacity = useTransform(smooth, [0.88, 1], [0, 1])

    // Right-edge side progress marker matching the 0, 0.425, 0.85 targets
    const renderSideProgress = (pi: number) => {
        let p: number[], fill: string[]
        if (pi === 0) {
            p = [0, 0.2]; fill = ['100%', '0%']
        } else if (pi === 1) {
            p = [0.2, 0.35, 0.5, 0.65]; fill = ['0%', '100%', '100%', '0%']
        } else {
            p = [0.65, 0.8]; fill = ['0%', '100%']
        }
        return useTransform(smooth, p, fill)
    }

    return (
        <div id="Game" ref={containerRef} style={{ height: `${PANELS.length * 150}vh` }} className="relative">
            {/* Sticky viewport */}
            <div
                className="sticky top-0 h-screen"
                style={{ overflow: 'hidden' }}
            >
                {/* Horizontal rail — each panel is a flex child width=100vw */}
                <motion.div
                    className="flex h-full"
                    style={{ x: railX, width: `${PANELS.length * 100}vw` }}
                >
                    {PANELS.map((panel, pi) => (
                        // Each panel is a flex child that takes exactly 100vw
                        <div key={panel.id} style={{ width: '100vw', height: '100%', flexShrink: 0, position: 'relative' }}>
                            <Panel panel={panel} pi={pi} smooth={smooth} total={PANELS.length} />
                        </div>
                    ))}
                </motion.div>

                {/* Right-edge progress indicators (Interactive HUD) */}
                <div className="absolute right-3 md:right-5 top-1/2 -translate-y-1/2 z-40 hidden sm:flex flex-col gap-3">
                    {PANELS.map((p, pi) => (
                        <div
                            key={pi}
                            onClick={() => {
                                const el = containerRef.current
                                if (!el) return
                                const sectionScrollable = el.offsetHeight - window.innerHeight
                                const target = el.offsetTop + (pi * 0.425) * sectionScrollable
                                window.scrollTo({ top: target, behavior: 'smooth' })
                            }}
                            className="relative rounded-full overflow-hidden bg-white/10 cursor-pointer group hover:bg-white/20 transition-colors"
                            style={{ width: '4px', height: '52px', padding: '0 4px', margin: '0 -4px' }} // Wider hit area
                            title={`Go to ${p.label}`}
                        >
                            <motion.div className="absolute top-0 left-1/2 -translate-x-1/2 w-[2px] rounded-full" style={{ height: renderSideProgress(pi), background: p.accent }} />
                        </div>
                    ))}
                </div>

                {/* Exit Overlay — Fades in at the exact end of Panel 3 to blend into the next light section */}
                <motion.div
                    className="absolute inset-0 pointer-events-none z-50 pointer-events-none"
                    style={{
                        opacity: exitOverlayOpacity,
                        background: 'linear-gradient(to bottom, transparent 0%, #ffffff 80%, #ffffff 100%)'
                    }}
                />
            </div>
        </div>
    )
}
