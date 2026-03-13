# How to set up biak for GitHub Actions?

* Run setup ([what?](Setup)) locally (if you haven't done it yet)
* Run disable ([what?](Disable)) locally (if you haven't done it yet)
* Add this step to your GitHub Action before `dotnet build`:
    ```yml
    - name: Install and run biak
      run: |
          dotnet tool install --global kurnakovv.biak
          dotnet biak
          dotnet biak enable
    ```

That's it - happy coding! 🚀

Also, see a real GitHub example ([where?](https://github.com/kurnakovv/biak-usage-examples/blob/main/.github/workflows/dotnet-build-with-analyzers.yml)) and see how it works in a real example ([where?](https://github.com/kurnakovv/biak-usage-examples/pull/1))
