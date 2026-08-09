"use client";

import { useEffect, useRef, useState } from "react";

import { useTranslations } from "next-intl";

interface CaptchaProps {
  onValidate: (isValid: boolean) => void;
  invalid?: boolean;
}

const generateCode = () => {
  const chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
  return Array.from({ length: 5 }, () =>
    chars.charAt(Math.floor(Math.random() * chars.length))
  ).join("");
};

export const Captcha = ({ onValidate, invalid }: CaptchaProps) => {
  const t = useTranslations();
  const [code, setCode] = useState("");
  const [input, setInput] = useState("");
  const canvasRef = useRef<HTMLCanvasElement>(null);

  const randomColor = () =>
    `rgb(${Math.floor(Math.random() * 100)}, ${Math.floor(
      Math.random() * 100
    )}, ${Math.floor(Math.random() * 100)})`;

  const drawCaptcha = (text: string) => {
    const canvas = canvasRef.current;
    if (!canvas) return;
    const ctx = canvas.getContext("2d");
    if (!ctx) return;

    ctx.clearRect(0, 0, canvas.width, canvas.height);

    ctx.fillStyle = "#f3f4f6";
    ctx.fillRect(0, 0, canvas.width, canvas.height);

    for (let i = 0; i < 7; i++) {
      ctx.strokeStyle = randomColor();
      ctx.beginPath();
      ctx.moveTo(Math.random() * canvas.width, Math.random() * canvas.height);
      ctx.lineTo(Math.random() * canvas.width, Math.random() * canvas.height);
      ctx.stroke();
    }

    for (let i = 0; i < 100; i++) {
      ctx.fillStyle = randomColor();
      ctx.beginPath();
      ctx.arc(
        Math.random() * canvas.width,
        Math.random() * canvas.height,
        1,
        0,
        2 * Math.PI
      );
      ctx.fill();
    }

    const fontList = ["Arial", "Georgia", "Verdana", "Courier New"];
    text.split("").forEach((char, i) => {
      const fontSize = 22 + Math.random() * 10;
      const font = fontList[Math.floor(Math.random() * fontList.length)];
      ctx.font = `${fontSize}px ${font}`;
      ctx.fillStyle = randomColor();

      const x = 20 + i * 25 + Math.random() * 4;
      const y = 30 + Math.random() * 10;
      const angle = (Math.random() - 0.5) * 0.6;

      ctx.save();
      ctx.translate(x, y);
      ctx.rotate(angle);
      ctx.fillText(char, 0, 0);
      ctx.restore();
    });
  };

  const refresh = () => {
    const newCode = generateCode();
    setCode(newCode);
    setInput("");
    drawCaptcha(newCode);
    onValidate(false);
  };

  useEffect(() => {
    refresh();
  }, []);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const val = e.target.value.toUpperCase();
    setInput(val);
    onValidate(val === code);
  };

  return (
    <div className="mt-4 mb-4">
      <canvas
        ref={canvasRef}
        width={160}
        height={60}
        className="shadow-sm mx-auto border rounded-md"
      />
      <input
        value={input}
        onChange={handleChange}
        placeholder={t("captcha.placeholder")}
        className={`
    mt-2 p-3 border rounded-xl outline-none w-full text-center placeholder:text-gray-gray8
    ${invalid ? "border-red-700" : "border-gray-300"}
  `}
      />

      <div className="mt-2 text-center">
        <button
          type="button"
          onClick={refresh}
          className="text-blue-button text-sm"
        >
          {t("captcha.refresh")}
        </button>
      </div>
    </div>
  );
};
