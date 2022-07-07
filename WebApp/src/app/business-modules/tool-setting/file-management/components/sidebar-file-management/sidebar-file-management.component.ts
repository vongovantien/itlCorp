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
    @ViewChild('one') d1: ElementRef;
    @Output() newItemEvent = new EventEmitter<string>();

    title: string;
    urlRedirect: UrlRedirectOptions[];
    constructor(private route: ActivatedRoute, private _router: Router, private _location: Location) {
    }
    ngOnChanges(changes: SimpleChanges): void {
        this.title = this.route.snapshot.data['title']
    }

    ngOnInit() {
    }

    ngAfterViewInit() {

    }

    changeBreadcrumb() {
        console.log(this.folderChild)
        if (this.folderName !== null && this.folderName !== undefined) {
            // if (this.folderChild !== null && this.folderChild !== undefined) {
            //     return this.title = this.route.snapshot.data['title'] + '/' + this.folderName + "/" + this.folderChild.folderName
            // }
            this.title = this.route.snapshot.data['title'] + '/' + this.folderName
        }
        else {
            this.title = this.route.snapshot.data['title']
        }
        // if (this.folderName !== undefined) {
        //     if (this.folderChild !== undefined) {
        //         return this.d1.nativeElement.insertAdjacentHTML('beforeend', `${this.folderName}/ ${this.folderChild.folderName}}`);
        //     }
        //     return this.d1.nativeElement.insertAdjacentHTML('beforeend', `${this.folderName}`);

        // }
        // else {
        //     this.d1.nativeElement.remove();
        // }
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
}

interface UrlRedirectOptions {
    title: string,
    path: string,
    folderName: string,
    objectId: string,
    folder: string
}
