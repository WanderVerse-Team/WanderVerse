import Link from 'next/link'

const Logo: React.FC = () => {
  return (
    <Link href="/" className="flex items-center gap-2 group">
      <div
        className="w-8 h-8 rounded-xl flex items-center justify-center text-white text-sm font-black shadow-lg group-hover:scale-110 transition-transform duration-300"
        style={{ background: 'linear-gradient(135deg, #f35b03, #7678ed)' }}
      >
        W
      </div>
      <span
        className="text-xl font-black"
        style={{
          background: 'linear-gradient(135deg, #f35b03, #3d348b)',
          WebkitBackgroundClip: 'text',
          WebkitTextFillColor: 'transparent',
          backgroundClip: 'text',
        }}
      >
        WanderVerse
      </span>
    </Link>
  )
}

export default Logo
