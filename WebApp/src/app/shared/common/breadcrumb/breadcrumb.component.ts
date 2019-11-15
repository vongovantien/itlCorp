import { Component, OnInit, AfterViewInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import findIndex from 'lodash/findIndex';
import filter from 'lodash/filter';

@Component({
    selector: 'app-breadcrumb',
    templateUrl: './breadcrumb.component.html',
    styleUrls: ['./breadcrumb.component.scss']
})
export class BreadcrumbComponent implements OnInit, AfterViewInit {

    parent_name = null;
    children_name = null;
    ActiveRoute: any[] = [];

    parentUrl: string = '';

    constructor(
        private _activedRoute: ActivatedRoute,
        private router: Router
    ) { }

    ngOnInit() {

    }

    ngAfterViewInit(): void {
        setTimeout(() => {
            const storagedRoutes = localStorage.getItem("ActiveRoute");
            this.ActiveRoute = storagedRoutes == null ? [] : JSON.parse(storagedRoutes);
            const currentURLParts = this.router.url.split("/");
            const currentRoute = currentURLParts[currentURLParts.length - 1];
            const moduleRoute = this._activedRoute.parent.snapshot.data;
            const componentRoute: { name: string, path: string, level: number } = <any>this._activedRoute.snapshot.data; // lấy data từ routing trong component

            componentRoute.path = currentRoute;

            const indexModule = findIndex(this.ActiveRoute, x => x.level === moduleRoute.level);
            const indexComponent = findIndex(this.ActiveRoute, x => x.level === componentRoute.level);

            if (indexModule === -1) {
                this.ActiveRoute.push(moduleRoute, componentRoute);
            } else {
                this.ActiveRoute[indexModule] = moduleRoute;
                if (indexComponent !== -1) {
                    this.ActiveRoute[indexComponent] = componentRoute;
                    this.ActiveRoute = filter(this.ActiveRoute, function (o: any) {
                        return o.level <= componentRoute.level;
                    });
                } else {
                    this.ActiveRoute.push(componentRoute);
                }
            }

            localStorage.setItem("ActiveRoute", JSON.stringify(this.ActiveRoute));
        }, 150);

    }

    navigateRoute(index: number) {
        let url = '';
        if (index !== this.ActiveRoute.length - 1 && index !== 0) {
            if (index === 1) {
                url = encodeURI('/home/' + this.ActiveRoute[0].path + '/' + this.ActiveRoute[index].path);
            } else {
                url = encodeURI(this.mapParamUrl(this.ActiveRoute, index));
            }
            this.router.navigateByUrl(url);
        } else {
            return null;
        }
    }

    mapParamUrl(params: any[], index: number) {
        let url: string = '/home';
        for (let i = 0; i <= index; i++) {
            url += '/' + this.getPathFromUrl(params[i].path);

        }
        return url;
    }

    getPathFromUrl(url: string) {
        return url.split(/[?#]/)[0];
    }
}
