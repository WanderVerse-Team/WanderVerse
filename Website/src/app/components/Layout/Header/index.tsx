'use client'
import { Key, useEffect, useRef, useState } from 'react'
import { usePathname } from 'next/navigation'
import { HeaderItem } from '@/app/types/menu'
import { headerData as headerDataImport } from '@/app/data/siteData'
import Logo from './Logo'
import HeaderLink from './Navigation/HeaderLink'
import MobileHeaderLink from './Navigation/MobileHeaderLink'
import { motion, AnimatePresence } from 'framer-motion'

const Header: React.FC = () => {
  const [navbarOpen, setNavbarOpen] = useState(false)
  const [sticky, setSticky] = useState(false)
  const [scrollProgress, setScrollProgress] = useState(0)

  const mobileMenuRef = useRef<HTMLDivElement>(null)

  const handleScroll = () => {
    setSticky(window.scrollY >= 60)
    const docH = document.documentElement.scrollHeight - window.innerHeight
    setScrollProgress(docH > 0 ? (window.scrollY / docH) * 100 : 0)
  }

  const handleClickOutside = (event: MouseEvent) => {
    if (
      mobileMenuRef.current &&
      !mobileMenuRef.current.contains(event.target as Node) &&
      navbarOpen
    ) {
      setNavbarOpen(false)
    }
  }

  useEffect(() => {
    window.addEventListener('scroll', handleScroll)
    document.addEventListener('mousedown', handleClickOutside)
    return () => {
      window.removeEventListener('scroll', handleScroll)
      document.removeEventListener('mousedown', handleClickOutside)
    }
  }, [navbarOpen])

  useEffect(() => {
    document.body.style.overflow = navbarOpen ? 'hidden' : ''
  }, [navbarOpen])

  const headerData = headerDataImport

  return (
    <>
      <header
        className={`fixed top-0 z-40 w-full transition-all duration-500 ${sticky
            ? 'backdrop-blur-xl bg-white/80 shadow-lg border-b border-white/40'
            : 'bg-transparent border-b border-transparent'
          }`}
      >
        {/* Scroll progress bar */}
        <div
          className="absolute bottom-0 left-0 h-0.5 transition-all duration-100"
          style={{
            width: `${scrollProgress}%`,
            background: 'linear-gradient(90deg, #f35b03, #7678ed)',
          }}
        />

        <div className={`transition-all duration-300 ${sticky ? 'py-0' : 'py-2'}`}>
          <div className="container mx-auto max-w-screen-xl flex items-center justify-between px-6">
            {/* Logo */}
            <div className={`transition-all duration-300 ${sticky ? 'py-3' : 'py-6'}`}>
              <Logo />
            </div>

            {/* Desktop Nav */}
            <nav className="hidden lg:flex items-center gap-2">
              {headerData.map((item, index) => (
                <HeaderLink key={index} item={item} />
              ))}
            </nav>

            {/* Right side */}
            <div className={`flex items-center gap-4 transition-all duration-300 ${sticky ? 'py-3' : 'py-6'}`}>
              {/* Download CTA */}
              <a href="#" className="hidden md:block">
                <motion.button
                  whileHover={{ scale: 1.04 }}
                  whileTap={{ scale: 0.97 }}
                  className="btn-ripple text-sm font-bold py-2.5 px-6 rounded-full text-white"
                  style={{ background: 'linear-gradient(135deg, #f35b03, #3d348b)' }}
                >
                  Download
                </motion.button>
              </a>

              {/* Mobile hamburger */}
              <button
                onClick={() => setNavbarOpen(!navbarOpen)}
                className="block lg:hidden p-2 rounded-xl hover:bg-black/5 transition-colors"
                aria-label="Toggle mobile menu"
              >
                <div className="w-6 flex flex-col gap-1.5">
                  <motion.span
                    animate={navbarOpen ? { rotate: 45, y: 8 } : { rotate: 0, y: 0 }}
                    className="block h-0.5 bg-black rounded-full"
                  />
                  <motion.span
                    animate={navbarOpen ? { opacity: 0 } : { opacity: 1 }}
                    className="block h-0.5 bg-black rounded-full"
                  />
                  <motion.span
                    animate={navbarOpen ? { rotate: -45, y: -8 } : { rotate: 0, y: 0 }}
                    className="block h-0.5 bg-black rounded-full"
                  />
                </div>
              </button>
            </div>
          </div>
        </div>

        {/* Mobile backdrop */}
        <AnimatePresence>
          {navbarOpen && (
            <motion.div
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              exit={{ opacity: 0 }}
              className="fixed inset-0 bg-black/50 z-40 lg:hidden"
              onClick={() => setNavbarOpen(false)}
            />
          )}
        </AnimatePresence>

        {/* Mobile sidebar */}
        <AnimatePresence>
          {navbarOpen && (
            <motion.div
              ref={mobileMenuRef}
              initial={{ x: '100%' }}
              animate={{ x: 0 }}
              exit={{ x: '100%' }}
              transition={{ type: 'spring', damping: 28, stiffness: 260 }}
              className="fixed top-0 right-0 h-full w-80 z-50 lg:hidden overflow-hidden"
              style={{
                background: 'linear-gradient(160deg, #3d348b 0%, #02398a 100%)',
              }}
            >
              {/* Decorative orb */}
              <div
                className="absolute -top-20 -right-20 w-64 h-64 rounded-full opacity-20"
                style={{ background: 'rgba(243,91,3,0.5)', filter: 'blur(40px)' }}
              />

              {/* Mobile header */}
              <div className="flex items-center justify-between p-6 border-b border-white/10">
                <span className="text-white text-2xl font-black">WanderVerse</span>
                <button
                  onClick={() => setNavbarOpen(false)}
                  className="w-8 h-8 rounded-full bg-white/10 flex items-center justify-center text-white hover:bg-white/20 transition-colors"
                  aria-label="Close menu"
                >
                  ✕
                </button>
              </div>

              <nav className="flex flex-col p-6 gap-2">
                {headerData.map((item: HeaderItem, index: Key | null | undefined) => (
                  <MobileHeaderLink key={index} item={item} />
                ))}
              </nav>

              {/* CTA in mobile */}
              <div className="px-6 pb-8 mt-4">
                <button
                  className="w-full py-3 rounded-2xl font-bold text-white text-sm"
                  style={{ background: 'linear-gradient(135deg, #f35b03, #f7b801)' }}
                >
                  🚀 Download WanderVerse
                </button>
              </div>
            </motion.div>
          )}
        </AnimatePresence>
      </header>
    </>
  )
}

export default Header
