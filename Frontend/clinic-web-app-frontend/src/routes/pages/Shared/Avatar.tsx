import {useEffect, useState} from "react";
import bffFetch from "../../../services/bffFetch.tsx";

interface AvatarProps{
    PhotoUrl?: string | null,
    TextIfPhotoNull: string,
    IsDirect?: boolean;
}

export function AvatarFromSource({PhotoUrl, TextIfPhotoNull, IsDirect} : AvatarProps){
    const [photoSrc, setPhotoSrc] = useState<string | null>();
    
    useEffect(() => {
        if (!PhotoUrl) {
            setPhotoSrc(null);
            return;
        }

        if (IsDirect) {
            setPhotoSrc(PhotoUrl);
            return;
        }
        

        bffFetch(PhotoUrl)
            .then(async (res) => {
                if (!res.ok) return null;
                return res.json();
            })
            .then(data => {
                if (!data || !data.url) {
                    setPhotoSrc(null);
                    return;
                }

                setPhotoSrc(data.url)
            })
            .catch(() => setPhotoSrc(null));
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