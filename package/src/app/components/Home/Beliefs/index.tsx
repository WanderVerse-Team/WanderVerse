'use client'
import React from 'react'
import Link from 'next/link'

const Beliefs = () => {
  return (
    <section className='bg-cover bg-center overflow-hidden'>
      <div className='container mx-auto max-w-7xl px-4'>
        <div className='grid grid-cols-1 lg:grid-cols-2 gap-5'>
          {/* COLUMN-1 */}

          <div className="bg-purple pt-12 px-10 sm:px-24 pb-52 md:pb-70 rounded-3xl bg-[url('/images/beliefs/swirls.svg')] bg-no-repeat bg-right-bottom">
            <p className='text-lg font-normal text-white tracking-widest mb-5 text-center sm:text-start uppercase'>
              CORE CONCEPT
            </p>
            <h3 className='text-white mb-5 text-center sm:text-start'>
              Gamification{' '}
              <span className='text-white/60'>
                of every lesson.
              </span>
            </h3>
            <p className='text-lg text-white/75 pt-2 mb-16 text-center sm:text-start'>
              Dives deeper into each concept in every lesson in the local syllabus, grabbing the expected learning outcomes and gamifying each learning point through levels.
            </p>
            <div className='text-center sm:text-start'>
              <Link
                href='#'
                className='text-xl py-5 px-14 mt-5 font-semibold text-white rounded-full duration-300 bg-primary border border-primary hover:bg-darkmode hover:border-darkmode'>
                OUTCOMES
              </Link>
            </div>
          </div>

          {/* COLUMN-2 */}
          <div className=''>
            <div className="bg-[#D6FFEB] pt-12 px-10 sm:px-24 pb-52 md:pb-70 rounded-3xl bg-[url('/images/beliefs/bg.svg')] bg-no-repeat bg-bottom">
              <p className='text-lg font-normal text-primary tracking-widest mb-5 text-center sm:text-start uppercase'>
                OUTCOME
              </p>
              <h3 className='text-black mb-5 text-center sm:text-start'>
                <span className='text-primary'>Effortless</span> and fun learning
              </h3>
              <p className='pt-2 mb-16 text-center sm:text-start text-black/75 text-lg'>
                -	Understanding lesson concepts through colorful graphics and live interaction
-	Extensive practice of each concept through playing “games” instead of “studying” or “learning”
-	Deep learning of theory and development of cognitive skills in addition to book-based learning in school
              </p>
              <div className='text-center sm:text-start'>
                <Link
                  href='#'
                  className='text-xl py-5 px-14 mt-5 font-semibold text-white rounded-full bg-primary border border-primary hover:bg-darkmode hover:border-darkmode'>
                  Learn more
                </Link>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
  )
}
export default Beliefs
