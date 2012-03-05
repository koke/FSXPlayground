//
//  ViewController.m
//  FSTrack
//
//  Created by Jorge Bernal on 3/1/12.
//  Copyright (c) 2012 Automattic. All rights reserved.
//

#import "ViewController.h"

@implementation ViewController {
    BOOL mapChanging;
}
@synthesize altitudeLabel, speedLabel, headingLabel;
@synthesize mapView;
@synthesize planeView;
@synthesize socket;
@synthesize lastId;
@synthesize center;

- (void)didReceiveMemoryWarning
{
    [super didReceiveMemoryWarning];
    // Release any cached data, images, etc that aren't in use.
}

#pragma mark - View lifecycle

- (NSArray *)readPacketFrom:(NSMutableString *)response {
    NSArray *result = nil;
    
    if ([response rangeOfString:@"\n"].location != NSNotFound) {
        // Found new line: process data
        NSRange range = [response rangeOfString:@"\n"];
        NSString *data = [response substringToIndex:range.location];
        [response replaceCharactersInRange:NSMakeRange(0, range.location + 1) withString:@""];
        result = [data componentsSeparatedByString:@"|"];
    }
    return result;
}

- (void)viewDidLoad
{
    [super viewDidLoad];
    self.socket = [Socket socket];
    self.lastId = 0;
    self.center = kCLLocationCoordinate2DInvalid;
    dispatch_async(dispatch_get_global_queue(DISPATCH_QUEUE_PRIORITY_DEFAULT, 0), ^(void) {
        [self.socket connectToHostName:@"192.168.2.210" port:4020];
        NSMutableData *response = [[NSMutableData alloc] init];
        NSMutableString *responseString = [[NSMutableString alloc] init];
        
        while ( [self.socket readData:response] )
        {
            NSString *received = [[NSString alloc] initWithData:response encoding:[NSString defaultCStringEncoding]];
            [responseString appendString:received];
            NSArray *info = [self readPacketFrom:responseString];
            NSArray *tmpInfo = nil;
            while ((tmpInfo = [self readPacketFrom:responseString])) {
                info = tmpInfo;
            }
            if (info != nil) {
                NSLog(@"%@", [info componentsJoinedByString:@"|"]);

                dispatch_async(dispatch_get_main_queue(), ^(void) {
                    UInt32 seqId = [[info objectAtIndex:0] intValue];
                    if (seqId > self.lastId) {
                        CLLocationCoordinate2D c = CLLocationCoordinate2DMake([[info objectAtIndex:1] doubleValue], [[info objectAtIndex:2] doubleValue]);
                        mapChanging = YES;
                        if (CLLocationCoordinate2DIsValid(self.center)) {
                            [self.mapView setCenterCoordinate:c animated:YES];
                        } else {
                            MKCoordinateSpan span = MKCoordinateSpanMake(0.1f, 0.1f);
                            [self.mapView setRegion:MKCoordinateRegionMake(c, span) animated:YES];
                            
                        }
                        self.center = c;
                        mapChanging = NO;
                        self.altitudeLabel.text = [NSString stringWithFormat:@"Altitude: %@ ft", [info objectAtIndex:3]];
                        self.speedLabel.text = [NSString stringWithFormat:@"Airspeed: %@ kt", [info objectAtIndex:4]];
                        self.headingLabel.text = [NSString stringWithFormat:@"Heading: %@°", [info objectAtIndex:6]];
                        CGFloat rotation = [[info objectAtIndex:7] doubleValue] * M_PI / 180.0;
                        self.planeView.transform = CGAffineTransformMakeRotation(rotation);
                        self.lastId = seqId;
                    }
                });

            }
        }                
    });
}

- (void)viewDidUnload
{
    [super viewDidUnload];
    // Release any retained subviews of the main view.
    // e.g. self.myOutlet = nil;
    self.altitudeLabel = nil;
    self.speedLabel = nil;
    self.headingLabel = nil;
    self.mapView = nil;
    [self.socket close];
    self.socket = nil;
}

- (BOOL)shouldAutorotateToInterfaceOrientation:(UIInterfaceOrientation)interfaceOrientation
{
    // Return YES for supported orientations
    return YES;
}

#pragma mark - MKMapViewDelegate

- (void)mapView:(MKMapView *)mapView regionDidChangeAnimated:(BOOL)animated {
    if (!mapChanging) {
        mapChanging = YES;
        CLLocationCoordinate2D mapCenter = self.mapView.centerCoordinate;
        if (CLLocationCoordinate2DIsValid(self.center)) {
            if (self.center.latitude != mapCenter.latitude || self.center.longitude != mapCenter.longitude) {
                [self.mapView setCenterCoordinate:self.center animated:YES];
            }
        }
        mapChanging = NO;
    }
}

@end
