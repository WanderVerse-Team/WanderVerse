'use client'
import React from 'react'
import Image from 'next/image'
import Link from 'next/link'

const Digital = () => {
  return (
  <section className='relative bg-cover bg-center overflow-visible'>
      <div className='container mx-auto max-w-7xl px-4'>
        <div className="rounded-3xl bg-primary bg-[url('/images/digital/bg.svg')] bg-no-repeat bg-right-top lg:pb-60 pb-32 relative">
          <div className='grid grid-cols-1 lg:grid-cols-1 xl:grid-cols-2'>
            {/* COLUMN-1 */}
            <div className='pt-24 lg:pl-24'>
              <p className='text-xl font-normal text-white mb-5 tracking-widest text-center lg:text-start uppercase mt-5'>
                WanderVerse
              </p>
              {/* <h2 className='text-white mb-8 leading-tight text-center lg:text-start lg:w-full'> */}
              <h2 className='text-base lg:text-5xl text-white mb-8 leading-relaxed text-center lg:text-start lg:max-w-5xl mx-auto lg:mx-0'>
                Jump into the world of Duo where learning is a game and each level makes you smarter
              </h2>
              <div className='text-center lg:text-start'>
              </div>
            </div>
          </div>
          {/* COLUMN-2 */}
          <div className='absolute -bottom-16 -right-20 xl:block hidden'>
            <Image
              src='/images/digital/girldoodle.svg'
              alt='doodle'
              width={815}
              height={691}
            />
          </div>
        </div>
      </div>
    </section>
  )
}
export default Digital
