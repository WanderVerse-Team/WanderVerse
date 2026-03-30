import { Manrope } from 'next/font/google'
import './globals.css'
import Header from '@/app/components/Layout/Header'
import Footer from '@/app/components/Layout/Footer'
import ScrollToTop from '@/app/components/ScrollToTop'
import Aoscompo from '@/utils/aos'
import CustomCursor from '@/app/components/CustomCursor'
const font = Manrope({ subsets: ['latin'] })

// at top-level of src/app/layout.tsx
export const metadata = {
  icons: {
    icon: '/favicon.ico',
    shortcut: '/favicon.ico',
    apple: '/favicon.png',
  },
}

export default function RootLayout({

  children,
}: Readonly<{
  children: React.ReactNode
}>) {
  return (
    <html lang='en' suppressHydrationWarning>
      <body className={`${font.className} cursor-none`}>
        <CustomCursor />
        <Aoscompo>
          <Header />
          {children}
          <Footer />
        </Aoscompo>
        <ScrollToTop />
      </body>
    </html>
  )
}
