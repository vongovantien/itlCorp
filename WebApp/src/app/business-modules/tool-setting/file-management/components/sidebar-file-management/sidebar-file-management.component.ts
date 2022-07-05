import { AfterViewInit, Component, Input, OnInit } from '@angular/core';
import { ActivatedRoute, Event, NavigationEnd, Router } from '@angular/router';
import { RoutingConstants } from '@constants';

@Component({
    selector: 'sidebar-file-management',
    templateUrl: './sidebar-file-management.component.html'
})
export class SidebarFileManagementComponent implements OnInit, AfterViewInit {
    @Input() folderName: string;
    title: string = "Accounting/";
    constructor(private route: ActivatedRoute, private _router: Router) {

    }

    ngAfterViewInit(): void {
        this.changeBreadcrumb();
    }

    ngOnInit() {
        this.changeBreadcrumb();
    }

    changeBreadcrumb() {
        this.title += this.folderName + "/";
        // if (this.folderName != null && this.folderName != undefined) {
        //     this.title = this.route.snapshot.data['title'] + "/" + this.folderName;
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
}
