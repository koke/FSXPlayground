//
//  ViewController.h
//  FSTrack
//
//  Created by Jorge Bernal on 3/1/12.
//  Copyright (c) 2012 Automattic. All rights reserved.
//

#import <UIKit/UIKit.h>
#import <MapKit/MapKit.h>
#import "Socket.h"

@interface ViewController : UIViewController<UIScrollViewDelegate,MKMapViewDelegate>
@property (strong) IBOutlet UILabel *altitudeLabel, *speedLabel, *headingLabel;
@property (strong) IBOutlet MKMapView *mapView;
@property (strong) IBOutlet UIImageView *planeView;
@property (strong) Socket *socket;
@property (nonatomic, assign) uint lastId;
@property (nonatomic, assign) CLLocationCoordinate2D center;
@end
