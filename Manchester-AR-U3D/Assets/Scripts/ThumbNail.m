#import <Foundation/Foundation.h>
#import <AVFoundation/AVFoundation.h>

void createThumbNail (const char* pVideoPath, const char* pVideoName)
{
    NSString *pVPath = [NSString stringWithUTF8String:pVideoPath];
    NSString *pVName = [NSString stringWithUTF8String:pVideoName];

    NSArray *documentsDirectory = NSSearchPathForDirectoriesInDomains(NSDocumentDirectory, NSUserDomainMask, YES);
    NSString *docPath = [documentsDirectory objectAtIndex:0];
    
    NSString *videoThumb = [NSString stringWithFormat:@"%@.jpg",pVName];
    NSString *thumbPath = [docPath stringByAppendingPathComponent:videoThumb];
    
    NSURL *url = [NSURL fileURLWithPath:pVPath];
    
    AVAsset *asset = [AVAsset assetWithURL:url];
    AVAssetImageGenerator *imageGenerator = [[AVAssetImageGenerator alloc]initWithAsset:asset];
    imageGenerator.appliesPreferredTrackTransform = YES;
    CMTime time = [asset duration];
    time.value = 0;
    CGImageRef imageRef = [imageGenerator copyCGImageAtTime:time actualTime:NULL error:NULL];
    UIImage *pThumbNail = [UIImage imageWithCGImage:imageRef];
    CGImageRelease(imageRef);
    
    if (pThumbNail != nil)
    {
        NSData *data;
        data = UIImageJPEGRepresentation(pThumbNail, 0.8);
        NSFileManager *fileManager = [NSFileManager defaultManager];
        if ([fileManager createFileAtPath:thumbPath contents:data attributes:nil]==YES)
        {
            NSLog(@"create thumb success %@", thumbPath);
        }
        else
        {
            NSLog(@"create thumb failed %@", thumbPath);
        }
    }else{
	    NSLog(@"create thumb failed");
    }
}
