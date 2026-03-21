'use client'
import { useState } from 'react'
import Link from 'next/link'
import { HeaderItem } from '../../../../types/menu'
import { motion, AnimatePresence } from 'framer-motion'

const MobileHeaderLink: React.FC<{ item: HeaderItem }> = ({ item }) => {
  const [submenuOpen, setSubmenuOpen] = useState(false)

  return (
    <div className="relative w-full border-b border-white/10 last:border-0">
      <Link
        href={item.href}
        onClick={item.submenu ? (e) => { e.preventDefault(); setSubmenuOpen(!submenuOpen) } : undefined}
        className="flex items-center justify-between w-full py-4 text-white font-semibold text-lg hover:text-yellow-400 transition-colors"
      >
        <span>{item.label}</span>
        {item.submenu && (
          <motion.span
            animate={{ rotate: submenuOpen ? 180 : 0 }}
            transition={{ duration: 0.25 }}
            className="text-white/40 text-xs"
          >
            ▼
          </motion.span>
        )}
      </Link>

      <AnimatePresence>
        {submenuOpen && item.submenu && (
          <motion.div
            initial={{ height: 0, opacity: 0 }}
            animate={{ height: 'auto', opacity: 1 }}
            exit={{ height: 0, opacity: 0 }}
            transition={{ duration: 0.3 }}
            className="overflow-hidden pl-4"
          >
            {item.submenu.map((subItem, index) => (
              <Link
                key={index}
                href={subItem.href}
                className="block py-2.5 text-white/60 hover:text-white text-base transition-colors"
              >
                → {subItem.label}
              </Link>
            ))}
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  )
}

export default MobileHeaderLink
