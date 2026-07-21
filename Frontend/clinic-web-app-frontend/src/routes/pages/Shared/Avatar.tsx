import {useEffect, useState} from "react";
import bffFetch from "../../../services/bffFetch.tsx";

interface AvatarProps{
    PhotoUrl?: string | null,
    TextIfPhotoNull: string,
    IsDirect?: boolean;
}

export function AvatarFromSource({PhotoUrl, TextIfPhotoNull, IsDirect} : AvatarProps){
    const [photoSrc, setPhotoSrc] = useState<string | null>(null);

    useEffect(() => {
        if (!PhotoUrl) {
            setPhotoSrc(null);
            return;
        }

        if (IsDirect) {
            setPhotoSrc(PhotoUrl);
        } else {
            bffFetch(PhotoUrl)
                .then(res => res.json())
                .then(data => setPhotoSrc(data.url))
                .catch(() => setPhotoSrc(null));
        }
        
    }, [PhotoUrl, IsDirect]);
    
    return (
        <div className="avatar">
            {photoSrc ? (
                <img src={photoSrc} alt={TextIfPhotoNull} onError={() => setPhotoSrc(null)} />
            ) : (
                <div className="avatar-placeholder">{TextIfPhotoNull|| '?'}</div>
            )}
        </div>
    );
}