"use client";
import { useState } from "react";
import Link from "next/link";
import { HeaderItem } from "../../../../types/menu";
import { usePathname } from "next/navigation";
import { motion, AnimatePresence } from "framer-motion";

const HeaderLink: React.FC<{ item: HeaderItem }> = ({ item }) => {
  const [submenuOpen, setSubmenuOpen] = useState(false);
  const path = usePathname();

  return (
    <div
      className="relative"
      onMouseEnter={() => item.submenu && setSubmenuOpen(true)}
      onMouseLeave={() => setSubmenuOpen(false)}
    >
      <Link
        href={item.href}
        className="nav-link relative text-base font-semibold text-black/70 hover:text-black transition-colors duration-200 px-3 py-2 rounded-lg hover:bg-black/5 flex items-center gap-1"
      >
        {item.label}
        {item.submenu && (
          <motion.span
            animate={{ rotate: submenuOpen ? 180 : 0 }}
            transition={{ duration: 0.2 }}
            className="text-xs opacity-50"
          >
            ▼
          </motion.span>
        )}
      </Link>

      <AnimatePresence>
        {submenuOpen && item.submenu && (
          <motion.div
            initial={{ opacity: 0, y: -8, scale: 0.96 }}
            animate={{ opacity: 1, y: 0, scale: 1 }}
            exit={{ opacity: 0, y: -8, scale: 0.96 }}
            transition={{ duration: 0.2, ease: [0.22, 1, 0.36, 1] }}
            className="absolute top-full left-0 mt-2 w-56 bg-white shadow-xl rounded-2xl border border-black/5 overflow-hidden z-50"
          >
            {item.submenu.map((subItem, index) => (
              <Link
                key={index}
                href={subItem.href}
                className="block px-4 py-3 text-sm font-medium text-black/70 hover:text-black hover:bg-primary/5 transition-colors border-b border-black/5 last:border-0"
              >
                {subItem.label}
              </Link>
            ))}
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
};

export default HeaderLink;
