'use client'
import { useEffect, useRef, useState } from 'react'
import { motion, useMotionValue, useSpring, AnimatePresence } from 'framer-motion'

export default function CustomCursor() {
    const [visible, setVisible] = useState(false)
    const [hovered, setHovered] = useState(false)
    const [clicked, setClicked] = useState(false)
    const [cursorText, setCursorText] = useState('')

    // Raw mouse position
    const mouseX = useMotionValue(-100)
    const mouseY = useMotionValue(-100)

    // Snappy inner dot
    const dotX = useSpring(mouseX, { stiffness: 700, damping: 35 })
    const dotY = useSpring(mouseY, { stiffness: 700, damping: 35 })

    // Laggy outer ring / bubble
    const ringX = useSpring(mouseX, { stiffness: 150, damping: 25 })
    const ringY = useSpring(mouseY, { stiffness: 150, damping: 25 })

    useEffect(() => {
        // Hide on touch devices
        if (window.matchMedia('(pointer: coarse)').matches) return

        const onMove = (e: MouseEvent) => {
            mouseX.set(e.clientX)
            mouseY.set(e.clientY)
            if (!visible) setVisible(true)

            // Dynamic Hit Testing (Event Delegation)
            const target = e.target as HTMLElement
            // Look up the DOM tree for interactive tags or data-cursor attributes
            const interactable = target.closest('a, button, [role="button"], input, textarea, label, select, [data-cursor]') as HTMLElement

            if (interactable) {
                setHovered(true)
                const text = interactable.getAttribute('data-cursor') || ''
                setCursorText(text)
            } else {
                setHovered(false)
                setCursorText('')
            }
        }

        const onLeave = () => setVisible(false)
        const onDown = () => setClicked(true)
        const onUp = () => setClicked(false)

        document.addEventListener('mousemove', onMove)
        document.addEventListener('mouseleave', onLeave)
        document.addEventListener('mousedown', onDown)
        document.addEventListener('mouseup', onUp)

        return () => {
            document.removeEventListener('mousemove', onMove)
            document.removeEventListener('mouseleave', onLeave)
            document.removeEventListener('mousedown', onDown)
            document.removeEventListener('mouseup', onUp)
        }
    }, [mouseX, mouseY, visible])

    const isTextMode = cursorText.length > 0

    return (
        <>
            {/* ── Outer glow ring / Text Bubble ── */}
            <motion.div
                className="fixed top-0 left-0 pointer-events-none z-[9999] rounded-full flex items-center justify-center overflow-hidden"
                style={{
                    x: ringX,
                    y: ringY,
                    translateX: '-50%',
                    translateY: '-50%',
                    opacity: visible ? 1 : 0,
                    // Remove blend mode when it's a solid text bubble
                    mixBlendMode: isTextMode ? 'normal' : 'multiply'
                }}
                animate={{
                    width: isTextMode ? 110 : hovered ? 56 : clicked ? 20 : 36,
                    height: isTextMode ? 110 : hovered ? 56 : clicked ? 20 : 36,
                    background: isTextMode
                        ? '#f35b03' // Solid bright orange for text
                        : hovered
                            ? 'radial-gradient(circle, rgba(118,120,237,0.55) 0%, rgba(243,91,3,0.15) 100%)'
                            : 'radial-gradient(circle, rgba(243,91,3,0.35) 0%, rgba(118,120,237,0.08) 100%)',
                    border: isTextMode
                        ? 'none'
                        : hovered
                            ? '2px solid rgba(118,120,237,0.7)'
                            : '2px solid rgba(243,91,3,0.5)',
                }}
                transition={{ type: 'spring', stiffness: 200, damping: 22, mass: 0.4 }}
            >
                <AnimatePresence mode="wait">
                    {isTextMode && (
                        <motion.span
                            key={cursorText}
                            initial={{ opacity: 0, scale: 0.5 }}
                            animate={{ opacity: 1, scale: 1 }}
                            exit={{ opacity: 0, scale: 0.5, transition: { duration: 0.1 } }}
                            className="text-white text-[10px] uppercase font-black tracking-widest text-center leading-tight"
                        >
                            {cursorText}
                        </motion.span>
                    )}
                </AnimatePresence>
            </motion.div>

            {/* ── Inner sharp dot (Hides in text mode) ── */}
            <motion.div
                className="fixed top-0 left-0 pointer-events-none z-[9999] rounded-full"
                style={{
                    x: dotX,
                    y: dotY,
                    translateX: '-50%',
                    translateY: '-50%',
                    opacity: visible && !isTextMode ? 1 : 0,
                }}
                animate={{
                    width: hovered ? 6 : clicked ? 14 : 8,
                    height: hovered ? 6 : clicked ? 14 : 8,
                    background: hovered ? '#7678ed' : '#f35b03',
                    scale: clicked ? 0.6 : 1,
                }}
                transition={{ type: 'spring', stiffness: 600, damping: 30 }}
            />
        </>
    )
}
