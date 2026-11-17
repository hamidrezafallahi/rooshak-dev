'use client';

import {
  createContext,
  type ReactNode,
  useContext,
  useEffect,
  useRef,
  useState,
} from 'react';

import MediaImage from '@components/atoms/MediaImage';

type SliderState = {
  index: number;
  count: number;
  setVisible: (visible: boolean) => void;
};

const SliderContext = createContext<SliderState>({ index: 0, count: 1, setVisible: () => {} });

/**
 * Holds the active slide index for one card and advances it every `interval` ms.
 * Pauses while the card is off-screen or the tab is hidden.
 * Always starts at 0 (the main image) on server and client → no hydration mismatch.
 */
export function CardSliderProvider({
  count,
  interval = 2000,
  children,
}: {
  count: number;
  interval?: number;
  children: ReactNode;
}) {
  const [index, setIndex] = useState(0);
  const [visible, setVisible] = useState(false);

  useEffect(() => {
    if (count < 2 || !visible) return;
    const id = window.setInterval(() => {
      if (document.visibilityState === 'visible') setIndex((i) => (i + 1) % count);
    }, interval);
    return () => window.clearInterval(id);
  }, [count, interval, visible]);

  return (
    <SliderContext.Provider value={{ index, count, setVisible }}>{children}</SliderContext.Provider>
  );
}

/** Stacked images (whole, object-contain); the active one fades in over the others. */
export function CardSliderImages({
  images,
  alt,
  priority,
  className = '',
}: {
  images: string[];
  alt: string;
  priority?: boolean;
  className?: string;
}) {
  const { index, setVisible } = useContext(SliderContext);
  const ref = useRef<HTMLDivElement>(null);

  // Only rotate while the card is actually on screen.
  useEffect(() => {
    const el = ref.current;
    if (!el || typeof IntersectionObserver === 'undefined') {
      setVisible(true);
      return;
    }
    const io = new IntersectionObserver(([entry]) => setVisible(entry.isIntersecting), {
      threshold: 0.3,
    });
    io.observe(el);
    return () => io.disconnect();
  }, [setVisible]);

  return (
    <div ref={ref} className={`absolute inset-0 ${className}`}>
      {images.map((src, i) => (
        <div
          key={`${src}-${i}`}
          aria-hidden={i !== index}
          className={[
            'absolute inset-0 transition-opacity duration-700 ease-in-out motion-reduce:transition-none',
            i === index ? 'opacity-100' : 'opacity-0',
          ].join(' ')}
        >
          <MediaImage
            src={src}
            alt={i === 0 ? alt : `${alt} - ${i + 1}`}
            fill
            priority={priority && i === 0}
            sizes="(max-width: 768px) 75vw, 240px"
            className="object-contain"
          />
        </div>
      ))}
    </div>
  );
}

/** One dot per image; the active dot is bright, the rest dimmed. */
export function CardSliderDots() {
  const { index, count } = useContext(SliderContext);

  return (
    <div aria-hidden className="flex justify-center items-center gap-[3px] mb-2">
      {Array.from({ length: count }, (_, i) => (
        <span
          key={i}
          className={[
            'h-[4px] w-[4px] rounded-full transition-colors duration-500',
            i === index ? 'bg-white' : 'bg-white/45',
          ].join(' ')}
        />
      ))}
    </div>
  );
}
