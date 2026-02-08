import './PaymentCard.css';

interface PaymentCardProps {
  name: string;
  cardNumber: string;
  expiration: string;
  securityCode: string;
  isFlipped: boolean;
}

export default function PaymentCard({
  name,
  cardNumber,
  expiration,
  securityCode,
  isFlipped,
}: PaymentCardProps) {
  const displayNumber = cardNumber || '#### #### #### ####';
  const displayName = name || 'FULL NAME';
  const displayExpiry = expiration || 'MM/YY';
  const displayCvv = securityCode || '***';

  return (
    <div className="creditcard-container">
      <div className={`creditcard${isFlipped ? ' flipped' : ''}`}>
        {/* Front */}
        <svg
          className="front"
          viewBox="0 0 350 220"
          xmlns="http://www.w3.org/2000/svg"
        >
          <defs>
            <linearGradient id="cardFrontGrad" x1="0%" y1="0%" x2="100%" y2="100%">
              <stop offset="0%" stopColor="#e3b7ac" />
              <stop offset="100%" stopColor="#945c67" />
            </linearGradient>
          </defs>
          {/* Card body */}
          <rect width="350" height="220" rx="15" fill="url(#cardFrontGrad)" />

          {/* Chip */}
          <rect x="30" y="60" width="45" height="35" rx="5" fill="#c1c4e3" stroke="#3f4b7f" strokeWidth="1" />
          <line x1="30" y1="72" x2="75" y2="72" stroke="#3f4b7f" strokeWidth="0.5" />
          <line x1="30" y1="82" x2="75" y2="82" stroke="#3f4b7f" strokeWidth="0.5" />
          <line x1="52" y1="60" x2="52" y2="95" stroke="#3f4b7f" strokeWidth="0.5" />

          {/* Contactless icon */}
          <g transform="translate(85, 68)" fill="none" stroke="#fff" strokeWidth="1.5" opacity="0.7">
            <path d="M2 10 A8 8 0 0 1 10 2" />
            <path d="M4 12 A12 12 0 0 1 16 0" />
            <path d="M6 14 A16 16 0 0 1 22 -2" />
          </g>

          {/* Card number */}
          <text className="number" x="30" y="130">{displayNumber}</text>

          {/* Cardholder label + name */}
          <text className="label" x="30" y="165">cardholder name</text>
          <text className="name" x="30" y="180">{displayName}</text>

          {/* Expiration label + value */}
          <text className="label" x="250" y="165">expiration</text>
          <text className="expiry" x="250" y="180">{displayExpiry}</text>

          {/* Decorative circles */}
          <circle cx="305" cy="35" r="18" fill="rgba(255,255,255,0.15)" />
          <circle cx="325" cy="45" r="14" fill="rgba(255,255,255,0.1)" />
        </svg>

        {/* Back */}
        <svg
          className="back"
          viewBox="0 0 350 220"
          xmlns="http://www.w3.org/2000/svg"
        >
          {/* Card body */}
          <rect width="350" height="220" rx="15" fill="#3f4b7f" />

          {/* Magnetic stripe */}
          <rect x="0" y="30" width="350" height="40" fill="#1a1f3d" />

          {/* Signature strip + CVV area */}
          <rect x="20" y="100" width="230" height="30" rx="3" fill="#e3b7ac" />
          <text className="back-name" x="30" y="120">{displayName}</text>

          <rect x="260" y="100" width="70" height="30" rx="3" fill="#fff" />
          <text className="label" x="275" y="110" fill="#3f4b7f">CVV</text>
          <text className="cvv-text" x="280" y="125">{displayCvv}</text>

          {/* Bottom text */}
          <text x="30" y="180" fontSize="7" fill="rgba(255,255,255,0.4)" fontFamily="'Courier New', monospace">
            Authorized Signature — Not Valid Unless Signed
          </text>

          {/* Decorative stripe */}
          <rect x="0" y="200" width="350" height="5" fill="#c1c4e3" opacity="0.3" />
        </svg>
      </div>
    </div>
  );
}
