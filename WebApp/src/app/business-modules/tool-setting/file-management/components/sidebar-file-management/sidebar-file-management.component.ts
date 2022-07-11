import { AfterViewInit, Component, Input, OnInit, OnChanges, SimpleChanges, ViewChild, ElementRef, EventEmitter, Output } from '@angular/core';
import { ActivatedRoute, Event, NavigationEnd, Router } from '@angular/router';
import { RoutingConstants } from '@constants';
import { Location } from '@angular/common';
@Component({
    selector: 'sidebar-file-management',
    templateUrl: './sidebar-file-management.component.html',
    styleUrls: ['./sidebar-file-management.component.scss']
})
export class SidebarFileManagementComponent implements OnChanges {

    @Input() listBreadcrumb: Array<object>;
    @Output() isDisplayDefaultFolder = new EventEmitter<string>();
    @Output() objectBack = new EventEmitter<any>();
    title: string;

    constructor(private route: ActivatedRoute, private _router: Router) {
    }
    ngOnChanges(changes: SimpleChanges): void {
        this.title = this.route.snapshot.data['title']
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

    onBreadcrumbActive(item: any) {
        if (item === "Accounting") {
            this.listBreadcrumb.splice(0, 2);
        }
        this.listBreadcrumb.pop();
        this.objectBack.emit(item)
    }
}

interface UrlRedirectOptions {
    title: string,
    path: string,
    folderName: string,
    objectId: string,
    folder: string
}
