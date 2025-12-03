export function AnimacionCriptos() {
  const canvas = document.getElementById('canvasCripto') as HTMLCanvasElement | null
  if (!canvas) return
  canvas.width = window.innerWidth
  canvas.height = window.innerHeight
  const ctx = canvas.getContext('2d')
  if (!ctx) return

  const simbolos = ['₿', 'Ξ', 'Ł', 'Ð', '₮', 'ⓩ', '₳', '₵', 'Ⓝ', '♦']
  const particles: {
    x: number
    y: number
    speed: number
    size: number
    symbol: string
  }[] = []

  for (let i = 0; i < 100; i++) {
    particles.push({
      x: Math.random() * canvas.width,
      y: Math.random() * -canvas.height,
      speed: 0.5 + Math.random() * 2,
      size: 15 + Math.random() * 15,
      symbol: simbolos[Math.floor(Math.random() * simbolos.length)],
    })
  }

  function animar() {
    if (!ctx || !canvas) return
    ctx.clearRect(0, 0, canvas.width, canvas.height)

    particles.forEach((p) => {
      ctx.font = `${p.size}px 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif`
      ctx.fillStyle = 'rgba(119, 119, 119, 0.4)'
      ctx.fillText(p.symbol, p.x, p.y)

      p.y += p.speed

      if (p.y > canvas.height) {
        p.y = Math.random() * -100
        p.x = Math.random() * canvas.width
      }
    })
    requestAnimationFrame(animar)
  }
  animar()
}


