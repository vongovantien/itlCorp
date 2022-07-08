import { AfterViewInit, Component, Input, OnInit, OnChanges, SimpleChanges, ViewChild, ElementRef, EventEmitter, Output } from '@angular/core';
import { ActivatedRoute, Event, NavigationEnd, Router } from '@angular/router';
import { RoutingConstants } from '@constants';
import { Location } from '@angular/common';
@Component({
    selector: 'sidebar-file-management',
    templateUrl: './sidebar-file-management.component.html'
})
export class SidebarFileManagementComponent implements OnInit, OnChanges {
    @Input() folderName: string;
    @Input() folderChild: any;
    @Input() listBreadcrumb: Array<object>;
    @Output() newItemEvent = new EventEmitter<string>();
    @Output() isDisplayDefaultFolder = new EventEmitter<string>();
    displayDefaultFolder: boolean;
    title: string;
    urlRedirect: UrlRedirectOptions[];
    constructor(private route: ActivatedRoute, private _router: Router, private _location: Location) {
    }
    ngOnChanges(changes: SimpleChanges): void {
        this.title = this.route.snapshot.data['title']
        console.log(this.listBreadcrumb)
    }

    ngOnInit() {

    }

    ngAfterViewInit() {

    }

    changeBreadcrumb() {
        this.title = this.route.snapshot.data['title']
    }

    navigateFileMngt(moduleUrl: string) {
        if (moduleUrl === 'accounting') {
            this._router.navigate([RoutingConstants.TOOL.FILE_MANAGMENT]);
        }
        else {
            this._router.navigate([RoutingConstants.TOOL.FILE_MANAGMENT + "/" + moduleUrl]);
        }
    }

    onChangeBreadcrumb() {
        this.newItemEvent.emit(this.folderName);
    }

    onBreadcrumbActive(item: any) {
        this.listBreadcrumb.pop()
    }
}

interface UrlRedirectOptions {
    title: string,
    path: string,
    folderName: string,
    objectId: string,
    folder: string
}
