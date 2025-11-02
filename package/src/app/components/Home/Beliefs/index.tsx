'use client'
import React from 'react'
import Link from 'next/link'

const Beliefs = () => {
  return (
    <section id='Game' className='bg-cover bg-center overflow-hidden'>
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
            <p className='text-2xl text-white/95 pt-2 mb-16 text-center sm:text-start'><br/>
              Dives deeper into each concept in every lesson in the local syllabus, grabbing the expected learning outcomes and gamifying each learning point through levels.
            </p>
          </div>

          {/* COLUMN-2 */}
          <div className=''>
            <div className="bg-[#f7b801] pt-12 px-10 sm:px-24 pb-52 md:pb-70 rounded-3xl bg-[url('/images/beliefs/bg.svg')] bg-no-repeat bg-bottom">
              <p className='text-lg font-normal text-[#000000] tracking-widest mb-5 text-center sm:text-start uppercase'>
                OUTCOME
              </p>
              <h3 className='text-black mb-5 text-center sm:text-start'>
                <span className='text-primary'>Effortless</span> and fun learning
              </h3>
              <p className='pt-2 mb-16 text-center sm:text-start text-black/75 text-lg'><br/>
                -	Understanding lesson concepts through colorful graphics and live interaction<br/><br/>
-	Extensive practice of each concept through playing "games" instead of "studying" or "learning"<br/><br/>
-	Deep learning of theory and development of cognitive skills in addition to book-based learning in school
              </p>
            </div>
          </div>
        </div>
      </div>
    </section>
  )
}
export default Beliefs
